using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.Builders.Ticket
{
    public class PolregioModelBuilder : TicketBuilder
    {
        protected PolregioTicket Model = new();
        public PolregioModelBuilder(IEnumerable<string> allLines) : base(allLines)
        {

        }
        public override Models.Tickets.Ticket Build()
        {
            string tempLine;
            tempLine = allLines.SkipWhile(x => !x.Contains("Ważny:")).Take(1).FirstOrDefault();
            Model.ValidDateStart = GetValidityDate(tempLine);

            tempLine = allLines.SkipWhile(x => !x.Contains("Rozkład jazdy")).Skip(1).FirstOrDefault();
            Model.DepartureDateTime = GetDepartureDateTime(tempLine, Model.ValidDateStart);
            Model.ArrivalDateTime = GetArrivalDateTime(tempLine, Model.ValidDateStart);
            Model.StartingStation = GetDepartureStation(tempLine);
            Model.FinalStation = GetArrivalStation(tempLine);

            tempLine = allLines.FirstOrDefault(l => l.Contains("Pociąg") && l.Contains("nr"));
            Model.TrainNumber = GetTrainNumber(tempLine);

            tempLine = allLines.FirstOrDefault(l => l.Contains("Właściciel:"));
            Model.TravelerName = GetTravelerName(tempLine);

            tempLine = allLines.FirstOrDefault(l => l.Contains("Bilet ważny") && l.Contains("od"));
            Model.ValidDurationInHours = GetValidityDuration(tempLine);

            return Model;
        }

        private int GetValidityDuration(string tempLine)
        {
            return String.IsNullOrEmpty(tempLine)
                ? throw new NotSupportedException(NotSupportedExMessage)
                : Convert.ToInt32(tempLine.Split(" ")[2].Trim());
        }

        private string GetTravelerName(string tempLine)
        {
            return String.IsNullOrEmpty(tempLine)
                ? throw new NotSupportedException(NotSupportedExMessage)
                : tempLine[12..tempLine.IndexOf("w tym")].Trim();
        }

        private string GetTrainNumber(string tempLine)
        {
            if (String.IsNullOrEmpty(tempLine))
            {
                throw new NotSupportedException(NotSupportedExMessage);
            }
            else
            {
                int index = tempLine.IndexOf("nr");

                return String.Concat(tempLine[7..index].Trim(), " ", tempLine[(index + 2)..].Trim());
            }
        }

        private string GetArrivalStation(string tempLine)
        {
            Match timeMatch = Regex.Match(tempLine[5..], TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return tempLine[(timeMatch.Index + 10)..].Trim();
        }

        private string GetDepartureStation(string tempLine)
        {
            Match timeMatch = Regex.Match(tempLine[5..], TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return tempLine[5..(timeMatch.Index + 5)].Trim();
        }

        private DateTime GetArrivalDateTime(string tempLine, DateTime validDateStart)
        {
            Match timeMatch = Regex.Match(tempLine[5..], TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return new DateTime(validDateStart.Year,
                    validDateStart.Month,
                    validDateStart.Day,
                    Convert.ToInt32(timeMatch.Value[..2]),
                    Convert.ToInt32(timeMatch.Value[3..]),
                    0);
        }

        protected DateTime GetValidityDate(string tempLine)
        {
            return String.IsNullOrWhiteSpace(tempLine) || tempLine.Length < 18
                ? throw new NotSupportedException("Validity date line not found or not valid.")
                : new DateTime(Convert.ToInt32(tempLine[13..17]), Convert.ToInt32(tempLine[10..12]), Convert.ToInt32(tempLine[7..9]));
        }

        private DateTime GetDepartureDateTime(string tempLine, DateTime validityDateTime)
        {
            string time = tempLine.Split(" ").First();
            return String.IsNullOrWhiteSpace(tempLine) || validityDateTime == default
                ? throw new NotSupportedException(NotSupportedExMessage)
                : new DateTime(validityDateTime.Year,
                    validityDateTime.Month,
                    validityDateTime.Day,
                    Convert.ToInt32(time[..2]),
                    Convert.ToInt32(time[3..5]),
                    0);
        }
    }
}
