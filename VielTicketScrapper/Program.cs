using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
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
                IntercityTicket ticket = (IntercityTicket)scrapper.ScrapPDF(filePath).ParseToTicket();

                switch (to)
                {
                    case ExportFileType.ICal:
                        string eventTitle = $"{ticket.TrainType} | {ticket.StartingStation} - {ticket.FinalStation}, {ticket.TravelerName}";
                        string eventDescription = $"Nr biletu: {ticket.TicketNumber}\n" +
                                                  $"Nr wagonu: {ticket.TrainCarNumber}\n" +
                                                  $"Miejsce: { ticket.Seat}\n" +
                                                  $"Czas podróży: {TimeSpan.FromTicks(ticket.ArrivalDateTime.Ticks - ticket.DepartureDateTime.Ticks):hh\\:mm} \n" +
                                                  $"Długość trasy: {ticket.TravelDistance} km\n";

                        string alarmMessage = $"Pociąg z {ticket.StartingStation} o godz. {ticket.DepartureDateTime:HH:mm}";

                        var iCal = ICal.Create()
                                        .AddEvent(eventTitle, ticket.DepartureDateTime, ticket.ArrivalDateTime)
                                        .AddEventDescription(eventDescription)
                                        .AddEventAlarm(15, alarmMessage)
                                        .AddEventAlarm(2 * 60, alarmMessage)
                                        .AddEventAlarm(24 * 60, alarmMessage);

                        File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + ticket.TicketNumber + ".ics", iCal.ToString());
                        cout("You should find iCal file next to the ticket file");
                        break;
                    default:
                        cout("No ExportFileType passed to switch-case. Something went really wrong...");
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
