using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
using VielTicketScrapper.Builders.Ticket;
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
                }.WithHandler(nameof(Intercity)),
                new Command("scrapFolder", "Scrap the data from Intercity ticket!")
                {
                    new Argument<string>("folderPath"),
                    new Option<ExportFileType>("--to", "ical/text"),
                    new Option<bool>("--verbose")
                }.WithHandler(nameof(ScrapFolder))
            };

            return await cmd.InvokeAsync(args);
        }

        private static void ScrapFolder(string folderPath, ExportFileType to, bool verbose, IConsole console)
        {
            if (verbose)
                coutVerbose($"About to scrap data from the folder [{folderPath}]");

            string[] allFiles = Directory.GetFiles(folderPath, "*.pdf");
            string outputDirectory = Path.Combine(folderPath, "Processed", DateTime.Now.ToString("yyyyMMddhhmmss"));
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            ICalEvent icsHandler = CalendarICSBuilder.Create();
            foreach(var filePath in allFiles)
            {
                Scrapper scrapper = new();
                scrapper.ScrapPDF(filePath);

                IntercityModelBuilder ticketBuilder = new(scrapper.allLines);
                try {
                    IntercityTicket ticket = ticketBuilder.Build();
                    if (verbose) coutVerbose($"Data from [{Path.GetFileName(filePath)}] scrapped sucessfully...");

                    switch (to)
                    {
                        case ExportFileType.ICal:
                            string eventTitle = $"{ticket.TrainType} {ticket.TrainNumber} | {ticket.StartingStation} - {ticket.FinalStation}, {ticket.TravelerName}";
                            string eventDescription = $"Nr biletu: {ticket.TicketNumber}\n" +
                                                      $"Nr wagonu: {ticket.TrainCarNumber}\n" +
                                                      $"Miejsce: { ticket.Seat}\n" +
                                                      $"Czas podróży: {TimeSpan.FromTicks(ticket.ArrivalDateTime.Ticks - ticket.DepartureDateTime.Ticks):hh\\:mm} \n" +
                                                      $"Długość trasy: {ticket.TravelDistance} km\n";

                            string alarmMessage = $"Pociąg z {ticket.StartingStation} o godz. {ticket.DepartureDateTime:HH:mm}";

                            icsHandler.AddEvent(eventTitle, ticket.DepartureDateTime, ticket.ArrivalDateTime)
                                      .AddEventDescription(eventDescription)
                                      .AddEventAlarm(15, alarmMessage)
                                      .AddEventAlarm(2 * 60, alarmMessage)
                                      .AddEventAlarm(24 * 60, alarmMessage);

                            if (verbose) coutVerbose($"{ticket.TicketNumber} processed sucessfully...");
                            File.Move(filePath, Path.Combine(outputDirectory, Path.GetFileName(filePath)), true);

                            break;
                        default:
                            coutError("No ExportFileType passed to switch-case.");
                            break;
                    }
                }
                catch (NotSupportedException e)
                {
                    coutError($"{Path.GetFileName(filePath)} is not valid Intercity Ticket file. System error: {e.Message}");
                    return;
                }
                catch (Exception e)
                {
                    coutError($"{Path.GetFileName(filePath)} is not valid Intercity Ticket file. System error: {e.Message}");
                    return;
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "IntercityTicketsEvents.ics"), icsHandler.ToString());
            coutSuccess($"All done! You should find .ics file in the [{outputDirectory}] directory.");
        }

        private static void Intercity(string filePath, ExportFileType to, bool verbose, IConsole console)
        {
            string fileName = Path.GetFileName(filePath);
            string folderPath = Path.GetDirectoryName(filePath);
            
            if (verbose)
                coutVerbose($"About to scrap data from the '{fileName}' file...");

            Scrapper scrapper = new();
            IntercityModelBuilder ticketBuilder = new(scrapper.allLines);
            IntercityTicket ticket = new();
            try
            {
                scrapper.ScrapPDF(filePath);
                 ticket = ticketBuilder.Build();
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

                    var iCal = CalendarICSBuilder.Create()
                                    .AddEvent(eventTitle, ticket.DepartureDateTime, ticket.ArrivalDateTime)
                                    .AddEventDescription(eventDescription)
                                    .AddEventAlarm(15, alarmMessage)
                                    .AddEventAlarm(2 * 60, alarmMessage)
                                    .AddEventAlarm(24 * 60, alarmMessage);

                    File.WriteAllText(folderPath + DateTime.Now.ToString("yyyyMMddhhmmss_") + ticket.TicketNumber + ".ics", iCal.ToString());
                    coutSuccess("You should find iCal file next to the ticket file");
                    break;
                default:
                    coutError("No ExportFileType passed to switch-case.");
                    break;
            }

            coutSuccess($"All done!");
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
