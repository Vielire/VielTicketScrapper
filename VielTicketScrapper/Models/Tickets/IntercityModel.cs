using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    class IntercityTicketModel
    {
        public string TicketNumber { get; set; }
        public decimal TicketPrice { get; set; }
        public string TicketPriceCurrency { get; set; }

        public string TravelerName { get; set; }
        public string StartingStation { get; set; }
        public string FinalStation { get; set; }
        public string TrainType { get; set; }
        public int TrainNumber { get; set; }
        public int TrainCarNumber { get; set; }
        public int TravelDistance { get; set; }
        public string Seat { get; set; }

#nullable enable
        public DateTime? StartDateTime { get; set; }
        public DateTime? StopDateTime { get; set; }
#nullable disable
    }
}
