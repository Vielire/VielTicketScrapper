﻿using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VielTicketScrapper.FileGenerators;
using VielTicketScrapper.Models.Enums;
using VielTicketScrapper.Models.Tickets;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapper
{
    public static class Program
    {
        delegate void CoutCustom(string message, ConsoleColor foregroundColor = ConsoleColor.DarkGray, ConsoleColor bgColor = ConsoleColor.Black);
        static readonly CoutCustom coutCustom = (message, foregroundColor, bgColor) =>
        {
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = foregroundColor;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        };

        static readonly Action<string> cout = (message) => Console.Out.WriteLine(message);
        static readonly Action<string> coutError = (message) => coutCustom(message, ConsoleColor.Red);
        static readonly Action<string> coutVerbose = (message) => coutCustom(message, ConsoleColor.DarkGray);
        static readonly Action<string> coutSuccess = (message) => coutCustom(message, ConsoleColor.DarkGreen);

        public static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Command("intercity", "Scrap the data from Intercity ticket!")
                {
                    new Argument<string>("filepath"),
                    new Option<ExportFileType>("--to", "ical/text"),
                    new Option<bool>("--verbose")
                }.WithHandler(nameof(Intercity))
            };

            return await cmd.InvokeAsync(args);

            //Intercity(@"C:\NO_BACKUP_FILES\Bilety\Intercity_template_wymiana.pdf", ExportFileType.ICal, false, null);
            //return 0;
        }
        private static void Intercity(string filePath, ExportFileType to, bool verbose, IConsole console)
        {
            string[] filepathParts = filePath.Split(Path.DirectorySeparatorChar);
            string fileName = filepathParts[filepathParts.Length-1];
            string folderPath = filePath.Substring(0, filePath.Length - fileName.Length);
            
            if (verbose)
                coutVerbose($"About to scrap data from the '{fileName}' file...");

            try
            {
                IntercityScrapper scrapper = new IntercityScrapper();
                TicketModel ticket = scrapper.ScrapPDF(filePath).ParseToTicket();

                ICal ical = new(ticket);

                switch (to)
                {
                    case ExportFileType.ICal:
                        File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + ticket.TicketNumber + ".ics", ical.ToString());
                        cout("You should find iCal file next to the ticket file");
                        break;
                    default:
                        File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + ticket.TicketNumber + ".txt", ical.ToString());
                        cout("You should find text file next to the ticket file");
                        break;
                }

                coutSuccess($"All done!");
            }
            catch(NotSupportedException e)
            {
                coutError(e.Message);
                Environment.Exit(1006); // ERROR_FILE_INVALID
            }
            catch(Exception e)
            {
                coutError(e.Message);
                Environment.Exit(13); //ERROR_INVALID_DATA
            }
        }

        private static Command WithHandler(this Command command, string methodName)
        {
            var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var handler = CommandHandler.Create(method);
            command.Handler = handler;
            return command;
        }
    }
}
