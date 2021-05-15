using System;
using System.Linq;
using System.Text.RegularExpressions;
using VielTicketScrapper.Models.Tickets;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace VielTicketScrapper.Builders.Ticket
{
    public class IntercityModelBuilder
    {
        private const string NotSupportedExMessage = "You provided file that is not supported within IntercityScrapper class";
        private const string TimeRegexPattern = @"[0-2]\d[:][0-5]\d";
        private const string DateRegexPattern = @"[0-3]\d[.][0-1]\d";

        protected IntercityTicket Model = new();

        private readonly IEnumerable<string> allLines;

        public IntercityModelBuilder(IEnumerable<string> allLines)
        {
            this.allLines = allLines;
        }
        public IntercityTicket Build()
        {
            //StartingStation, DepartureDateTime, TrainType, TrainNumber, TravelDistance, TicketPrice, TicketPriceCurrency
            string multiDataLine_StartStation = allLines.SkipWhile(x => !x.Contains("Stacja Data Godzina")).Skip(1).FirstOrDefault();
            //FinalStation, ArrivalDateTime, TrainCarNumber
            string multiDataLine_FinalStation = allLines.SkipWhile(x => !x.Contains("Stacja Data Godzina")).Skip(2).FirstOrDefault();

            if (multiDataLine_StartStation == default || multiDataLine_FinalStation == default)
                throw new NotSupportedException(NotSupportedExMessage);

            Model.TicketNumber = GetTicketNumber();
            Model.TravelerName = GetTravelerName();
            Model.PaidDate = GetPaymentDate();

            Model.DepartureDateTime = GetDateTime(multiDataLine_StartStation, Model.PaidDate);
            Model.StartingStation = GetStationName(multiDataLine_StartStation);
            Model.TrainType = GetTrainType(multiDataLine_StartStation);
            Model.TrainNumber = GetTrainNumber(multiDataLine_StartStation);
            Model.TravelDistance = GetTravelDistance(multiDataLine_StartStation);
            Model.TicketPrice = GetTicketPrice(multiDataLine_StartStation);
            Model.TicketPriceCurrency = GetTicketPriceCurrency(multiDataLine_StartStation);
            Model.Seat = GetSeat(multiDataLine_StartStation);

            Model.ArrivalDateTime = GetDateTime(multiDataLine_FinalStation, Model.PaidDate);
            Model.FinalStation = GetStationName(multiDataLine_FinalStation);
            Model.TrainCarNumber = GetTrainCarNumber(multiDataLine_FinalStation);

            return Model;
        }

        protected DateTime GetPaymentDate()
        {
            string ticketLine = allLines.SkipWhile(line => !line.Contains("Zapłacono i wystawiono")).Skip(1).FirstOrDefault();
            return String.IsNullOrWhiteSpace(ticketLine)
                ? throw new NotSupportedException("Payment date not found.")
                : new DateTime(Convert.ToInt32(ticketLine[..4]), Convert.ToInt32(ticketLine[5..7]), Convert.ToInt32(ticketLine[8..10]));
        }
        protected string GetTicketNumber()
        {
            string ticketLine = allLines.Where(line => line.Contains("Nr biletu : ")).FirstOrDefault();
            return String.IsNullOrWhiteSpace(ticketLine)
                ? "Ticket number not found"
                : ticketLine.Split("Nr biletu : ")[1];
        }
        protected string GetTravelerName()
        {
            string travelerLine = allLines.Where(line => line.Contains("Podróżny")).FirstOrDefault();
            return String.IsNullOrWhiteSpace(travelerLine)
                ? "No traveler found"
                : travelerLine.Split(": ")[1];
        }
        protected static DateTime GetDateTime(string line, DateTime paymentDay)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            Match dateMatch = Regex.Match(line, DateRegexPattern);

            if (!timeMatch.Success || !dateMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            //There is no year of departure or arrival on the ticket, but there is constraint on the
            //Intercity site, that tickets can be purchased up to one month (30 days) before departure.
            //It is also not possible to buy a ticket whose departure date and time have passed.
            //Therefore, we can assume that the date of departure should be a maximum of one month later
            //than the date of purchase of the ticket.
            DateTime dt = new DateTime(paymentDay.Year,
                                Convert.ToInt32(dateMatch.Value.Substring(3, 2)),
                                Convert.ToInt32(dateMatch.Value.Substring(0, 2)),
                                Convert.ToInt32(timeMatch.Value.Substring(0, 2)),
                                Convert.ToInt32(timeMatch.Value.Substring(3, 2)),
                                0);

            if (dt.Date >= paymentDay.Date)
                return dt;
            else
            {
                do
                {
                    dt = dt.AddYears(1);
                    if (dt.Year > paymentDay.Year && (dt < paymentDay || (dt.Subtract(paymentDay).TotalDays > 35)))
                        throw new InvalidDataException("The day of buying the ticket is later than the date of departure, which is impossible for Intercity ticket system.");

                } while (dt < paymentDay);
                return dt;
            }
        }
        protected static string GetStationName(string line)
        {
            Match dateMatch = Regex.Match(line, DateRegexPattern);
            if (!dateMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line.Substring(0, dateMatch.Index).Trim();
        }

        protected static string GetTrainType(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line[(timeMatch.Index + 6)..].Split(" ")[0];
        }
        protected static int GetTrainNumber(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return Convert.ToInt32(line[(timeMatch.Index + 6)..].Split(" ")[1]);
        }
        protected static int GetTravelDistance(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return Convert.ToInt32(line[(timeMatch.Index + 6)..].Split(" ")[2]);
        }
        protected static string GetSeat(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            string seat = line[(timeMatch.Index + 6)..].Split(" ")[3];
            string seatNumber = seat[0..^1];
            string seatType = seat[^1..];

            return seatType == "o" ? seatNumber + " okno"
                 : seatType == "ś" ? seatNumber + " środek"
                 : seatType == "k" ? seatNumber + " korytarz"
                 : null;
        }
        protected static decimal GetTicketPrice(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            char decimalSeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            return Convert.ToDecimal(line[(timeMatch.Index + 6)..].Split(" ")[4].Replace(',', decimalSeparator));
        }
        protected static string GetTicketPriceCurrency(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line[(timeMatch.Index + 6)..].Split(" ")[5] == "zł" ? "PLN" : "N/A"; ;
        }
        protected static int GetTrainCarNumber(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return Convert.ToInt32(line[(timeMatch.Index + 6)..].Split(" ")[0]);
        }
    }
}
