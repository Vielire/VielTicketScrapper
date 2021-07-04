using iText.Layout.Element;
using System;
using System.Collections.Generic;

namespace VielTicketScrapper.Models.Calendar
{
    public class TripEvent
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<EventAlarm> Alarms { get; set; } = new();
        
        public TripEvent WithAlarm(EventAlarm alarm)
        {
            Alarms.Add(alarm);
            return this;
        }

        public class EventAlarm
        {
            public int MinutesBeforeEvent { get; }
            public string Message { get; }

            public EventAlarm(int minutesBeforeEvent, string message)
            {
                MinutesBeforeEvent = minutesBeforeEvent;
                Message = message;
            }
        }
    }
}
