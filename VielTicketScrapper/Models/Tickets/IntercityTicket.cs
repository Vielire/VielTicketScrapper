using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Tickets
{
    public class IntercityTicket : Ticket
    {
        public string TrainType { get; set; }
        public int TrainNumber { get; set; }
        public int? TrainCarNumber { get; set; }
        public int TravelDistance { get; set; }
        public string Seat { get; set; }
        public DateTime PaidDate { get; set; }

        public override string GetEventDesc()
        {
            return String.Concat($"Nr biletu: {TicketNumber}\n",
                    TrainCarNumber != null ? $"Nr wagonu: {TrainCarNumber}\n" : "",
                    $"Miejsce: {Seat} \n",
                    $"Czas podróży: {TimeSpan.FromTicks(ArrivalDateTime.Ticks - DepartureDateTime.Ticks):hh\\:mm} \n",
                    $"Długość trasy: {TravelDistance} km\n");
        }

        public override string GetEventTitle()
        {
            return $"{TrainType} {TrainNumber} | {StartingStation} - {FinalStation}, {TravelerName}";
        }
    }
}
