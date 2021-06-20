using System;
using System.Linq;
using System.Text.RegularExpressions;
using VielTicketScrapper.Models.Tickets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace VielTicketScrapper.Builders.Ticket
{
    public class IntercityModelBuilder : TicketBuilder
    {
        protected IntercityTicket Model = new();

        public override string NotSupportedExMessage => "You provided file that is not supported within IntercityScrapper class";

        public IntercityModelBuilder(IEnumerable<string> allLines) : base(allLines)
        {
            
        }
        public override Models.Tickets.Ticket Build()
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
            
            Model.Seat = GetSeats(multiDataLine_StartStation, multiDataLine_FinalStation);

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
            int travelerLineIndex = allLinesAsList.FindIndex(line => line.Contains("Podróżny"));
            if (travelerLineIndex == -1)
            {
                return "No traveler found";
            }

            string travelerName = allLinesAsList[travelerLineIndex].Split(": ")[1];

            if (!allLinesAsList[travelerLineIndex + 1].StartsWith("Informacja o cenie"))
            {
                travelerName = String.Concat(travelerName, " ", allLinesAsList[travelerLineIndex + 1]);
            }

            return travelerName;
        }
        protected DateTime GetDateTime(string line, DateTime paymentDay)
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
        protected string GetStationName(string line)
        {
            Match dateMatch = Regex.Match(line, DateRegexPattern);
            if (!dateMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line.Substring(0, dateMatch.Index).Trim();
        }

        protected string GetTrainType(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line[(timeMatch.Index + 6)..].Split(" ").First();
        }
        protected string GetTrainNumber(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line[(timeMatch.Index + 6)..].Split(" ")[1];
        }
        protected int GetTravelDistance(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return Convert.ToInt32(line[(timeMatch.Index + 6)..].Split(" ")[2]);
        }
        protected string GetSeats(string startingStationLine, string finalStationLine)
        {
            if (finalStationLine.Contains("Bez gwarancji"))
            {
                return "Bez gwarancji miejsca do siedzenia";
            }
            else
            {
                Match timeMatch = Regex.Match(startingStationLine, TimeRegexPattern);
                if (!timeMatch.Success)
                    throw new NotSupportedException(NotSupportedExMessage);

                string[] textPartsAfterTime = startingStationLine[(timeMatch.Index + 6)..].Split(" ");
                List<string> seats = new();
                for (int i = 3; i < textPartsAfterTime.Length - 2; i++)
                {
                    string seatOnTicket = textPartsAfterTime[i].Replace(",", "");
                    string seatIndicator = seatOnTicket[^1..];
                    string seatType = seatIndicator == "o" ? " okno"
                     : seatIndicator == "ś" ? " środek"
                     : seatIndicator == "k" ? " korytarz" :
                     "";
                    seats.Add(seatOnTicket[..^1] + seatType);
                }

                return seats.Count > 0 ? String.Join(", ", seats) : "No seat found on the ticket."; 
            }
        }
        protected decimal GetTicketPrice(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            char decimalSeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            string textAfterTime = line[(timeMatch.Index + 6)..^3];
            int indexOfLastSpace = textAfterTime.LastIndexOf(' ');
            string priceShouldBeHere;
            if (indexOfLastSpace != -1)
            {
                priceShouldBeHere = textAfterTime[(indexOfLastSpace + 1)..];
                return Convert.ToDecimal(priceShouldBeHere.Replace(',', decimalSeparator));
            }
            else
            {
                throw new NotSupportedException(NotSupportedExMessage);
            }
        }
        protected string GetTicketPriceCurrency(string line)
        {
            Match timeMatch = Regex.Match(line, TimeRegexPattern);
            if (!timeMatch.Success)
                throw new NotSupportedException(NotSupportedExMessage);

            return line[(timeMatch.Index + 6)..].Split(" ").Last() == "zł" ? "PLN" : "N/A";
        }
        protected int? GetTrainCarNumber(string line)
        {
            if (line.Contains("Bez gwarancji"))
            {
                return null;
            }
            else
            {
                Match timeMatch = Regex.Match(line, TimeRegexPattern);
                if (!timeMatch.Success)
                    throw new NotSupportedException(NotSupportedExMessage);

                return Convert.ToInt32(line[(timeMatch.Index + 6)..].Split(" ").First());
            }
            
        }
    }
}
