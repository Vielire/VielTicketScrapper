using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Builders;
using VielTicketScrapper.Builders.Ticket;
using VielTicketScrapper.Models.Tickets;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapper.Processors
{
    public static class TicketToIcsProcessor
    {
        public static void Intercity(IntercityTicket ticket, ref ICalendarICSBuilder iCSBuilder)
        {
                string eventTitle = $"{ticket.TrainType} {ticket.TrainNumber} | {ticket.StartingStation} - {ticket.FinalStation}, {ticket.TravelerName}";
                string eventDescription = $"Nr biletu: {ticket.TicketNumber}\n" +
                                            $"Nr wagonu: {ticket.TrainCarNumber}\n" +
                                            $"Miejsce: {ticket.Seat} \n" +
                                            $"Czas podróży: {TimeSpan.FromTicks(ticket.ArrivalDateTime.Ticks - ticket.DepartureDateTime.Ticks):hh\\:mm} \n" +
                                            $"Długość trasy: {ticket.TravelDistance} km\n";

                string alarmMessage = $"Pociąg z {ticket.StartingStation} o godz. {ticket.DepartureDateTime:HH:mm}";

                iCSBuilder.AddEvent(eventTitle, ticket.DepartureDateTime, ticket.ArrivalDateTime)
                            .AddEventDescription(eventDescription)
                            .AddEventAlarm(15, alarmMessage)
                            .AddEventAlarm(2 * 60, alarmMessage)
                            .AddEventAlarm(24 * 60, alarmMessage);
        }
    }
}
