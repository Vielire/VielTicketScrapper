using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    class IntercityTicket : Ticket
    {
        public string TrainType { get; set; }
        public int TrainNumber { get; set; }
        public int TrainCarNumber { get; set; }
        public int TravelDistance { get; set; }
        public string Seat { get; set; }
        public DateTime PaidDate { get; set; }
    }
}
