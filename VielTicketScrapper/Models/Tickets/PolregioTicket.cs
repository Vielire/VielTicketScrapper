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
        public double? ValidDurationInHours { get; set; }
        public string TrainNumber { get; set; }

        public override string GetEventDesc()
        {
            return String.Concat($"Nr biletu: {TicketNumber}\n",
                    $"Czas podróży: {TimeSpan.FromTicks(ArrivalDateTime.Ticks - DepartureDateTime.Ticks):hh\\:mm} \n",
                    $"Ważny w dniu: {ValidDateStart.ToLongDateString()} \n",
                    ValidDurationInHours!=null ? $"Ważny przez: {TimeSpan.FromHours((double)ValidDurationInHours):hh\\:mm} \n" : "");;
        }

        public override string GetEventTitle()
        {
            return $"{TrainNumber} | {StartingStation} - {FinalStation}, {TravelerName}";
        }
    }
}
