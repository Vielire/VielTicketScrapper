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

        public override string GetEventDesc()
        {
            throw new NotImplementedException();
        }

        public override string GetEventTitle()
        {
            throw new NotImplementedException();
        }
    }
}
