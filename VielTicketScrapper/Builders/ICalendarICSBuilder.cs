using System;

namespace VielTicketScrapper.Builders
{
    public interface ICalendarICSBuilder
    {
        ICalendarICSBuilder AddEvent(string title, DateTime eventStart, DateTime eventEnd);
        ICalendarICSBuilder AddEventAlarm(int minutesBeforeEvent, string withMessage);
        ICalendarICSBuilder AddEventDescription(string description);
    }
}