using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.Builders.Ticket
{
    public class KolejeSlaskieModelBuilder : TicketBuilder
    {
        private const string DATE_FORMAT = "dd.MM.yyyy";
        private const int DEFAULT_TICKET_VALIDITY_DURATION = 6;
        protected KolejeSlaskieTicket Model = new();

        public KolejeSlaskieModelBuilder(IEnumerable<string> allLines) : base(allLines)
        {
        }

        public override Models.Tickets.Ticket Build()
        {
            string tempLine, tempLine2;
            tempLine = allLines.SkipWhile(x => !x.Contains("OD DO KL")).Skip(3).FirstOrDefault();
            // Assign Starting and Final station to this property due to impossible distinction of the stations on the ticket.
            Model.StartingStation = CleanStartingAndFinalStationLine(tempLine);

            tempLine = allLines.FirstOrDefault(x => x.Contains("Bilet ważny") && x.Contains("godz"));
            Model.ValidDurationInHours = GetValidityDuration(tempLine);

            tempLine2 = allLines.FirstOrDefault(x => x.Contains("Wyjazd dn.:"));
            Model.DepartureDateTime = GetDepartureDate(tempLine2, tempLine);
            Model.ArrivalDateTime = GetArrivalDate(Model.DepartureDateTime, Model.ValidDurationInHours);
            Model.ValidDateStart = Model.DepartureDateTime;

            Model.TravelerName = GetTravelerName(allLines.FirstOrDefault(x => x.Contains("Przejazd TAM")));
            Model.TicketNumber = GetTicketNumber(allLines.FirstOrDefault(x => x.Contains("PTU") && x.Contains("w tym")));

            return Model;
        }

        private string GetTicketNumber(string tempLine)
        {
            if (String.IsNullOrEmpty(tempLine)) return null;

            return String.Join(' ', tempLine.Split(" ")[^2..]);
        }

        private string GetTravelerName(string tempLine)
        {
            IEnumerable<string> splittedReversedLine = tempLine.Split(" ").Reverse();
            StringBuilder sb = new();
            foreach(string linePart in splittedReversedLine)
            {
                if(double.TryParse(linePart, out _))
                {
                    break;
                }
                sb.Insert(0, String.Concat(linePart, " "));
            }

            return sb.ToString().Trim();
        }

        private DateTime GetArrivalDate(DateTime departureDateTime, double? validDurationInHours)
        {
            validDurationInHours = validDurationInHours == null ? DEFAULT_TICKET_VALIDITY_DURATION : validDurationInHours;
            return departureDateTime.AddHours((double)validDurationInHours);
        }

        private DateTime GetDepartureDate(string lineWithDate, string lineWithTime)
        {
            if (String.IsNullOrEmpty(lineWithDate) || String.IsNullOrEmpty(lineWithTime))
                throw new NotSupportedException(NotSupportedExMessage);

            try
            {
                DateTime dt = DateTime.ParseExact(lineWithDate.Split(" ")[2], DATE_FORMAT, CultureInfo.InvariantCulture);
                string time = lineWithTime.Split("od: ")[1].Split(" ")[0];

                return dt.AddHours(Convert.ToInt32(time[..2]))
                       .AddMinutes(Convert.ToInt32(time[3..]));
            }
            catch(FormatException)
            {
                throw;
            }
        }

        private static double? GetValidityDuration(string tempLine)
        {
            try
            {
                return Convert.ToDouble(tempLine.Split(" ")[2].Trim());
            }
            catch
            {
                return null;
            }
        }

        private string CleanStartingAndFinalStationLine(string tempLine)
        {
            return String.IsNullOrEmpty(tempLine)
                ? throw new NotSupportedException(NotSupportedExMessage)
                : tempLine[4..^6].Trim();
        }
    }
}
