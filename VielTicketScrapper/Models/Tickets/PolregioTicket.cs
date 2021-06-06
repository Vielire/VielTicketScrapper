using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    public class PolregioTicket : Ticket
    {
        public DateTime ValidDateStart { get; set; }
        public int ValidDurationInHours { get; set; }
        public string TrainNumber { get; set; }

        public override string GetEventDesc()
        {
            return $"Nr biletu: {TicketNumber}\n" +
                    $"Czas podróży: {TimeSpan.FromTicks(ArrivalDateTime.Ticks - DepartureDateTime.Ticks):hh\\:mm} \n" +
                    $"Ważny w dniu: {ValidDateStart.ToLongDateString()} \n" +
                    $"Ważny przez: {TimeSpan.FromHours(ValidDurationInHours):hh\\:mm} \n";
        }

        public override string GetEventTitle()
        {
            return $"{TrainNumber} | {StartingStation} - {FinalStation}, {TravelerName}";
        }
    }
}
