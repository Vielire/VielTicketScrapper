using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
using VielTicketScrapper.Helpers;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapperService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        string folderIn;
        string outputDirectory;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            folderIn = _config.GetValue<string>("defaultFolder");
            _logger.LogInformation($"Worker will use {folderIn} as folder path.");

            if (!Directory.Exists(folderIn))
            {
                Directory.CreateDirectory(folderIn);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IEnumerable<string> allFiles = RetrieveFilePaths(folderIn);

                if (allFiles.Any())
                {
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    ICalendarICSBuilder iCSBuilder = CalendarICSBuilder.Create();

                    foreach (string filePath in allFiles)
                    {
                        try
                        {
                            Scrapper scrapper = new();
                            scrapper.Scrap(filePath);

                            var ticketBuilder = TicketIdentifier.InstantiateTicketBuilder(scrapper);
                            var ticket = ticketBuilder.Build();

                            iCSBuilder.AddEvent(ticket.GetEventTitle(), ticket.DepartureDateTime, ticket.ArrivalDateTime)
                                        .AddEventDescription(ticket.GetEventDesc())
                                        .AddEventAlarm(15, ticket.GetAlarmMessage())
                                        .AddEventAlarm(2 * 60, ticket.GetAlarmMessage())
                                        .AddEventAlarm(24 * 60, ticket.GetAlarmMessage());

                            File.Move(filePath, Path.Combine(outputDirectory, Path.GetFileName(filePath)), true);
                            _logger.LogInformation($"{Path.GetFileName(filePath)} file processed...");
                        }
                        catch (Exception e)
                        {
                            string notSupportedFilesFolderPath = Path.Combine(Directory.GetParent(outputDirectory).Parent.FullName, "NotSupportedFiles");
                            if (!Directory.Exists(notSupportedFilesFolderPath))
                            {
                                Directory.CreateDirectory(notSupportedFilesFolderPath);
                            }
                            File.Move(filePath, Path.Combine(notSupportedFilesFolderPath, Path.GetFileName(filePath)), true);
                            _logger.LogError($"[ERR] {e.Message}");
                        }
                    }

                    if (iCSBuilder.EventsCount > 0) { 
                        File.WriteAllText(Path.Combine(outputDirectory, "IntercityTicketsEvents.ics"), iCSBuilder.ToString());
                    }
                }
                else
                {
                    _logger.LogInformation("[INF] No new tickets were found.");
                }
                await Task.Delay(20000, stoppingToken);
            }
        }

        public List<string> RetrieveFilePaths(string path)
        {
            List<string> allFiles = new();

            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                outputDirectory = Path.Combine(path, "Processed", DateTime.Now.ToString("yyyyMMddhhmmss"));

                allFiles = Directory.GetFiles(path, "*.pdf").ToList<string>();
            }
            else
            {
                outputDirectory = Path.Combine(Path.GetDirectoryName(path), "Processed", DateTime.Now.ToString("yyyyMMddhhmmss"));

                if (Path.GetExtension(path) != ".pdf")
                {
                    Environment.Exit(1006); // ERROR_FILE_INVALID
                }
                allFiles.Add(path);
            }

            return allFiles;
        }
    }
}
