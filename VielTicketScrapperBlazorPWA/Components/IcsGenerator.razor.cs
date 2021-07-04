using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
using VielTicketScrapper.Helpers;
using VielTicketScrapper.Models.Calendar;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapperBlazorPWA.Components
{
    public partial class IcsGenerator
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ILogger<IcsGenerator> Logger { get; set; }

        private List<IBrowserFile> loadedFiles = new();
        private long maxFileSize = 1024 * 1024 * 5;
        private int maxAllowedFiles = 10;
        private bool isLoading;

        private List<string> exceptionMessages = new();

        private void LoadFiles(InputFileChangeEventArgs e)
        {
            isLoading = true;
            loadedFiles.Clear();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                try
                {
                    loadedFiles.Add(file);
                }
                catch (Exception ex)
                {
                    Logger.LogError("File: {Filename} Error: {Error}",
                        file.Name, ex.Message);
                }
            }

            isLoading = false;
        }

        private async Task ProcessFiles()
        {
            exceptionMessages.Clear();
            NewCalendarICSBuilder iCSBuilder = new();

            foreach (var file in loadedFiles)
            {
                try
                {
                    Scrapper scrapper = new();
                    System.IO.MemoryStream ms = new();
                    await file.OpenReadStream().CopyToAsync(ms);
                    ms.Position = 0;
                    scrapper.Scrap(ms);

                    var ticketBuilder = TicketIdentifier.InstantiateTicketBuilder(scrapper);
                    var ticket = ticketBuilder.Build();

                    var tripEvent = new TripEvent()
                    {
                        Title = ticket.GetEventTitle(),
                        Description = ticket.GetEventDesc(),
                        StartDateTime = ticket.DepartureDateTime,
                        EndDateTime = ticket.ArrivalDateTime,
                        Alarms = new()
                        {
                            new TripEvent.EventAlarm(15, ticket.GetAlarmMessage()),
                            new TripEvent.EventAlarm(2 * 60, ticket.GetAlarmMessage()),
                            new TripEvent.EventAlarm(24 * 60, ticket.GetAlarmMessage())
                        }
                    };

                    iCSBuilder.AddEvent(tripEvent);
                }
                catch (Exception e)
                {
                    exceptionMessages.Add(e.Message);
                }
            }

            if (iCSBuilder.EventsCount > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(iCSBuilder.ToString());

                await JSRuntime.InvokeVoidAsync("BlazorDownloadFile", "EventsFromTicket.ics", "text/plain", bytes);
            }
        }
    }
}
