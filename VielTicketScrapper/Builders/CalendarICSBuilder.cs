using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VielTicketScrapper.Builders
{
    interface ICalEvent
    {
        ICalEvent AddEvent(string title, DateTime eventStart, DateTime eventEnd);
        ICalEvent AddEventAlarm(int minutesBeforeEvent, string withMessage);
        ICalEvent AddEventDescription(string description);
    }
    class CalendarICSBuilder : ICalEvent
    {
        private Calendar calendar;
        private CalendarEvent CalendarEventHolder;

        private CalendarICSBuilder()
        {
            // Outlook needs property Method = CalendarMethods.Publish cause "REQUEST" will
            // update an existing event with the same UID (Unique ID) and a newer timestamp.
            calendar = new() { Method = CalendarMethods.Publish };
        }

        public static ICalEvent Create()
        {
            return new CalendarICSBuilder();
        }

        public ICalEvent AddEvent(string title, DateTime eventStart, DateTime eventEnd)
        {
            AppendCurrentEvent();

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

        public ICalEvent AddEventDescription(string description)
        {
            CalendarEventHolder.Description = description;
            return this;
        }

        public ICalEvent AddEventAlarm(int minutesBeforeEvent, string withMessage)
        {
            CalendarEventHolder.Alarms.Add(new()
            {
                Action = AlarmAction.Display,
                Trigger = new Trigger(TimeSpan.FromTicks(CalendarEventHolder.Start.AddMinutes(-minutesBeforeEvent).Ticks)),
                Summary = withMessage
            });
            return this;
        }

        public override string ToString()
        {
            AppendCurrentEvent();

            return new CalendarSerializer(new SerializationContext()).SerializeToString(calendar);
        }

        /// <summary>
        /// Appends current Event to the calendar object and sets CalendarEventHolder to null
        /// </summary>
        private void AppendCurrentEvent()
        {
            if (CalendarEventHolder != null) { 
                calendar.Events.Add(CalendarEventHolder);
                CalendarEventHolder = null;
            }
        }
    }
}