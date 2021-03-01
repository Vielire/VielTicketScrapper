using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Enums;
using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.FileGenerators
{
    class ICal
    {
        private readonly TicketModel ticket;

        public ICal(TicketModel ticket)
        {
            this.ticket = ticket;
        }

        public override string ToString()
        {
            Calendar cal = new();
            cal.Method = CalendarMethods.Publish; // Outlook needs this property "REQUEST" will update an existing event with the same UID (Unique ID) and a newer time stamp.

            CalendarEvent e = new()
            {
                Class = "Publish",
                Name = "VEVENT",
                Summary = $"{ticket.StartingStation} - {ticket.FinalStation}, {ticket.TravelerName}",
                Start = new CalDateTime(ticket.DepartureDateTime),
                End = new CalDateTime(ticket.ArrivalDateTime),
                Uid = Guid.NewGuid().ToString(),
                Description = $"Nr biletu: {ticket.TicketNumber}\n"
            };

            if(ticket is IntercityTicketModel intercityTicket)
            {
                e.Summary = $"{intercityTicket.TrainType} | " + e.Summary;
                e.Description += $"Nr wagonu: {intercityTicket.TrainCarNumber}\n";
                e.Description += $"Miejsce: {intercityTicket.Seat}\n";
                e.Description += $"Długość trasy: {intercityTicket.TravelDistance} km\n";
            }

            e.Description += $"Czas podróży: {TimeSpan.FromTicks(ticket.ArrivalDateTime.Ticks - ticket.DepartureDateTime.Ticks):hh\\:mm} \n";

            //Alarm 1 day before event
            e.Alarms.Add(new()
            {
                Action = AlarmAction.Display,
                Trigger = new Trigger(TimeSpan.FromTicks(ticket.DepartureDateTime.AddDays(-1).Ticks)),
                Summary = $"Pociąg do {ticket.FinalStation}"
            });

            //Alarm 2 hours before event
            e.Alarms.Add(new()
            {
                Action = AlarmAction.Display,
                Trigger = new Trigger(TimeSpan.FromTicks(ticket.DepartureDateTime.AddHours(-2).Ticks)),
                Summary = $"Pociąg do {ticket.FinalStation}"
            });

            cal.Events.Add(e);
            return new CalendarSerializer(new SerializationContext()).SerializeToString(cal);
        }
    }
}
