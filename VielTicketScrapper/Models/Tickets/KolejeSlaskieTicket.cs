using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    public class KolejeSlaskieTicket : Ticket
    {
        public DateTime ValidDateStart { get; set; }
        public double? ValidDurationInHours { get; set; }
        public override string GetEventDesc()
        {
            return String.Concat($"Nr biletu: {TicketNumber}\n",
                    $"Ważny w dniu: {ValidDateStart.ToLongDateString()} \n",
                    ValidDurationInHours != null ? $"Ważny przez: {TimeSpan.FromHours((double)ValidDurationInHours):hh\\:mm} \n" : ""); ;
        }

        public override string GetEventTitle()
        {
            return $"KŚ | {StartingStation}, {TravelerName}";
        }
    }
}
