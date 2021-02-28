using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    abstract class TicketModel
    {
        public string TicketNumber { get; set; }
        public decimal TicketPrice { get; set; }
        public string TicketPriceCurrency { get; set; }

        public string TravelerName { get; set; }
        public string StartingStation { get; set; }
        public string FinalStation { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
    }
}
