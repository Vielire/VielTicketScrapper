using System;

namespace VielTicketScrapper.Builders
{
    public interface ICalendarICSBuilder
    {
        int EventsCount { get; set; }
        ICalendarICSBuilder AddEvent(string title, DateTime eventStart, DateTime eventEnd);
        ICalendarICSBuilder AddEventAlarm(int minutesBeforeEvent, string withMessage);
        ICalendarICSBuilder AddEventDescription(string description);
    }
}