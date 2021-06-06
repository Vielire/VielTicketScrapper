using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    public abstract class Ticket
    {
        public string TicketNumber { get; set; }
        public decimal TicketPrice { get; set; }
        public string TicketPriceCurrency { get; set; }

        public string TravelerName { get; set; }
        public string StartingStation { get; set; }
        public string FinalStation { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }

        public virtual string GetAlarmMessage() { 
            return $"Pociąg z {StartingStation} o godz. {DepartureDateTime:HH:mm}";
        }

        public abstract string GetEventTitle();
        public abstract string GetEventDesc();
    }
}
