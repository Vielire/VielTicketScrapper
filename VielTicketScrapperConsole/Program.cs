using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
using VielTicketScrapper.Helpers;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapperConsole
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
                new Command("convert", "Convert PDF Intercity ticket(s) into .ics file! You need to provide file")
                {
                    new Argument<string>("path"),
                    new Option<bool>("--verbose")
                }.WithHandler(nameof(Convert))
            };

            return await cmd.InvokeAsync(args);
        }

        private static void Convert(string path, bool verbose, IConsole console)
        {
            List<string> allFiles = new();
            string outputDirectory;

            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                outputDirectory = Path.Combine(path, "Processed", DateTime.Now.ToString("yyyyMMddhhmmss"));

                if (verbose)
                    coutVerbose($"About to scrap data from the folder [{path}]");

                allFiles = Directory.GetFiles(path, "*.pdf").ToList<string>();
            }
            else
            {
                outputDirectory = Path.Combine(Path.GetDirectoryName(path), "Processed", DateTime.Now.ToString("yyyyMMddhhmmss"));

                if (verbose)
                    coutVerbose($"About to scrap data from the file [{Path.GetFileName(path)}]");

                if (Path.GetExtension(path) != ".pdf")
                {
                    coutError("Only PDF files are supported!");
                    Environment.Exit(1006); // ERROR_FILE_INVALID
                }
                allFiles.Add(path);
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            ICalendarICSBuilder iCSBuilder = CalendarICSBuilder.Create();

            foreach (string filePath in allFiles)
            {
                Scrapper scrapper = new();
                scrapper.ScrapPDF(filePath);

                var ticketBuilder = TicketIdentifier.InstantiateTicketBuilder(scrapper);
                var ticket = ticketBuilder.Build();

                iCSBuilder.AddEvent(ticket.GetEventTitle(), ticket.DepartureDateTime, ticket.ArrivalDateTime)
                            .AddEventDescription(ticket.GetEventDesc())
                            .AddEventAlarm(15, ticket.GetAlarmMessage())
                            .AddEventAlarm(2 * 60, ticket.GetAlarmMessage())
                            .AddEventAlarm(24 * 60, ticket.GetAlarmMessage());

                File.Copy(filePath, Path.Combine(outputDirectory, Path.GetFileName(filePath)), true);
            }

            File.WriteAllText(Path.Combine(outputDirectory, "IntercityTicketsEvents.ics"), iCSBuilder.ToString());
            coutSuccess($"All done! You should find .ics file in the [{outputDirectory}] directory which should be opened by now.");

            ProcessStartInfo startInfo = new()
            {
                Arguments = outputDirectory,
                FileName = "explorer.exe"
            };

            System.Diagnostics.Process.Start(startInfo);
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
