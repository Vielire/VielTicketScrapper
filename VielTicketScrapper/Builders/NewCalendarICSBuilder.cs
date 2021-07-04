using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Calendar;

namespace VielTicketScrapper.Builders
{
    public class NewCalendarICSBuilder
    {
        private readonly Calendar _calendar;

        public int EventsCount
        {
            get { return _calendar.Events.Count; }
        }

        public NewCalendarICSBuilder()
        {
            // Outlook needs property Method = CalendarMethods.Publish cause "REQUEST" will
            // update an existing event with the same UID (Unique ID) and a newer timestamp.
            _calendar = new() { Method = CalendarMethods.Publish };
        }

        public NewCalendarICSBuilder AddEvent(TripEvent tripEvent)
        {
            CalendarEvent calendarEvent = new()
            {
                Class = "Publish",
                Name = "VEVENT",
                Uid = Guid.NewGuid().ToString(),
                Summary = tripEvent.Title,
                Start = new CalDateTime(tripEvent.StartDateTime),
                End = new CalDateTime(tripEvent.EndDateTime),
                Description = tripEvent.Description
            };

            foreach(var alarm in tripEvent.Alarms)
            {
                calendarEvent.Alarms.Add(new() {
                    Action = AlarmAction.Display,
                    Trigger = new Trigger(TimeSpan.FromTicks(calendarEvent.Start.AddMinutes(-alarm.MinutesBeforeEvent).Ticks)),
                    Summary = alarm.Message
                });
            }

            _calendar.Events.Add(calendarEvent);

            return this;
        }

        public override string ToString()
        {
            if(_calendar.Events.Count > 0)
            {
                return new CalendarSerializer(new SerializationContext()).SerializeToString(_calendar);
            }
            else
            {
                throw new InvalidOperationException("Sequence of Trip Events contains no elements");
            }
        }
    }
}
