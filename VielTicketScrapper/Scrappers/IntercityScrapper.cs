using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VielTicketScrapper.Models.Tickets;
using System.Threading;

namespace VielTicketScrapper.Scrappers
{
    class IntercityScrapper : Scrapper
    {
        private const string NotSupportedExMessage = "You provided file that is not supported within IntercityScrapper class";
        private const string TimeRegexPattern = @"[0-2]\d[:][0-5]\d";
        private const string DateRegexPattern = @"[0-3]\d[.][0-1]\d";

        private IntercityTicket Model = new();

        public override IntercityTicket ParseToTicket()
        {
            //StartingStation, DepartureDateTime, TrainType, TrainNumber, TravelDistance, TicketPrice, TicketPriceCurrency
            string multiDataLine_StartStation = allLines.SkipWhile(x => !x.Contains("Stacja Data Godzina")).Skip(1).FirstOrDefault();
            //FinalStation, ArrivalDateTime, TrainCarNumber
            string multiDataLine_FinalStation = allLines.SkipWhile(x => !x.Contains("Stacja Data Godzina")).Skip(2).FirstOrDefault();

            if (multiDataLine_StartStation == default || multiDataLine_FinalStation == default)
                throw new NotSupportedException(NotSupportedExMessage);

            Model.TicketNumber = GetTicketNumber();
            Model.TravelerName = GetTravelerName();

            Model.DepartureDateTime = GetDateTime(multiDataLine_StartStation);
            Model.StartingStation = GetStationName(multiDataLine_StartStation);
            Model.TrainType = GetTrainType(multiDataLine_StartStation);
            Model.TrainNumber = GetTrainNumber(multiDataLine_StartStation);
            Model.TravelDistance = GetTravelDistance(multiDataLine_StartStation);
            Model.TicketPrice = GetTicketPrice(multiDataLine_StartStation);
            Model.TicketPriceCurrency = GetTicketPriceCurrency(multiDataLine_StartStation);
            Model.Seat = GetSeat(multiDataLine_StartStation);

            Model.ArrivalDateTime = GetDateTime(multiDataLine_FinalStation);
            Model.FinalStation = GetStationName(multiDataLine_FinalStation);
            Model.TrainCarNumber = GetTrainCarNumber(multiDataLine_FinalStation);
            
            return Model;
        }
        protected string GetTicketNumber()
        {
            string ticketLine = allLines.Where(line => line.Contains("Nr biletu : ")).FirstOrDefault();
            return String.IsNullOrWhiteSpace(ticketLine)
                ? "Ticket number not found"
                : ticketLine.Split("Nr biletu : ")[1].Split(" ")[0];
        }
        protected string GetTravelerName()
        {
            string travelerLine = allLines.Where(line => line.Contains("Podróżny")).FirstOrDefault();
            return String.IsNullOrWhiteSpace(travelerLine)
                ? "No traveler found"
                : travelerLine.Split(": ")[1];
        }
        protected static DateTime GetDateTime(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            Match dateMatch = Regex.Match(line, DateRegexPattern);

            if(!timeMatch.Success || !dateMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return new DateTime(DateTime.Now.Year,
                                Convert.ToInt32(dateMatch.Value.Substring(3, 2)),
                                Convert.ToInt32(dateMatch.Value.Substring(0, 2)),
                                Convert.ToInt32(timeMatch.Value.Substring(0, 2)),
                                Convert.ToInt32(timeMatch.Value.Substring(3, 2)),
                                0);
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
