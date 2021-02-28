using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VielTicketScrapper.FileGenerators;
using VielTicketScrapper.Models.Enums;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapper
{
    public static class Program
    {
        static readonly Action<string> cout = (message) => Console.Out.WriteLine(message);

        delegate void VerboseCout(string message, ConsoleColor foregroundColor = ConsoleColor.DarkGray, ConsoleColor bgColor = ConsoleColor.Black);
        static readonly VerboseCout coutVerbose = (message, foregroundColor, bgColor) =>
        {
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = foregroundColor;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        };
        public static async Task<int> Main(string[] args)
        {
            //var cmd = new RootCommand
            //{
            //    new Command("intercity", "Scrap the data from Intercity ticket!")
            //    {
            //        new Argument<string>("filepath"),
            //        new Option<ExportFileType>("--to", "ical/text"),
            //        new Option<bool>("--verbose")
            //    }.WithHandler(nameof(Intercity))
            //};

            //return await cmd.InvokeAsync(args);

            Intercity(@"C:\NO_BACKUP_FILES\Bilety\Intercity_template.pdf", ExportFileType.ICal, false, null);
            return 0;
        }
        private static void Intercity(string filePath, ExportFileType to, bool verbose, IConsole console)
        {
            string[] filepathParts = filePath.Split(Path.DirectorySeparatorChar);
            string fileName = filepathParts[filepathParts.Length-1];
            string folderPath = filePath.Substring(0, filePath.Length - fileName.Length);
            
            if (verbose)
                coutVerbose($"About to scrap data from the '{fileName}' file...");


            IntercityScrapper scrapper = new IntercityScrapper();
            scrapper.Scrap(filePath);
            ICal ical = new ICal(scrapper.Model);

            switch (to)
            {
                case ExportFileType.ICal:
                    File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + scrapper.Model.TicketNumber + ".ics", ical.ToString());
                    cout("You should find iCal file next to the ticket file");
                    break;
                default:
                    File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + scrapper.Model.TicketNumber + ".txt", ical.ToString());
                    cout("You should find text file next to the ticket file");
                    break;
            }
            
            if (verbose)
                coutVerbose($"All done!", ConsoleColor.DarkGreen);
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
