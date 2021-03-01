using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VielTicketScrapper.Builders
{
    interface IEventCreate
    {
        IEventOptions AddEvent(string title, DateTime eventStart, DateTime eventEnd);
    }
    interface IEventOptions : IEventCreate
    {
        IEventOptions AddEventAlarm(int minutesBefore, string withMessage);
        IEventOptions AddEventDescription(string description);
    }
    class ICal : IEventCreate, IEventOptions
    {
        private readonly Calendar calendar;
        private CalendarEvent CalendarEventHolder;
        
        private IList<CalendarEvent> CalendarEvents {get; set;}

        private ICal()
        {
            // Outlook needs property Method = CalendarMethods.Publish cause "REQUEST" will
            // update an existing event with the same UID (Unique ID) and a newer timestamp.
            calendar = new() { Method = CalendarMethods.Publish };
            CalendarEvents = new List<CalendarEvent>();
        }

        public static IEventCreate Create()
        {
            return new ICal();
        }

        public IEventOptions AddEvent(string title, DateTime eventStart, DateTime eventEnd)
        {
            if (CalendarEventHolder != null)
                CalendarEvents.Add(CalendarEventHolder);

            CalendarEventHolder = new()
            {
                Class = "Publish",
                Name = "VEVENT",
                Uid = Guid.NewGuid().ToString(),
                Summary = title,
                Start = new CalDateTime(eventStart),
                End = new CalDateTime(eventEnd),
            };
            return this;
        }

        public IEventOptions AddEventDescription(string description)
        {
            CalendarEventHolder.Description = description;
            return this;
        }

        public IEventOptions AddEventAlarm(int minutesBeforeDeparture, string withMessage)
        {
            CalendarEventHolder.Alarms.Add(new()
            {
                Action = AlarmAction.Display,
                Trigger = new Trigger(TimeSpan.FromTicks(CalendarEventHolder.Start.AddMinutes(-minutesBeforeDeparture).Ticks)),
                Summary = withMessage
            });
            return this;
        }

        public override string ToString()
        {
            if (CalendarEventHolder != null) {
                CalendarEvents.Add(CalendarEventHolder);
                CalendarEventHolder = null;
            };

            if (CalendarEvents.Any()) { 
                foreach (CalendarEvent e in CalendarEvents)
                {
                    calendar.Events.Add(e);
                }
                return new CalendarSerializer(new SerializationContext()).SerializeToString(calendar);
            }
            else
            {
                return "No events in calendar object found";
            }
        }
    }
}