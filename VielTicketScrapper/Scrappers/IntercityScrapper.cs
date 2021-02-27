using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Tickets;
using System.Threading;

namespace VielTicketScrapper.Scrappers
{
    class IntercityScrapper
    {
        private IEnumerable<string> allLines;
        private IntercityTicketModel Model { get; set; }
        private const string timeRegexPattern = @"[0-2]\d[:][0-5]\d";
        private const string dateRegexPattern = @"[0-3]\d[.][0-1]\d";

        public IntercityScrapper()
        {
            Model = new IntercityTicketModel();
        }
        public IntercityScrapper Scrap(string filePath)
        {
            StringBuilder processed = new StringBuilder();
            var pdfDocument = new PdfDocument(new PdfReader(filePath));
            var strategy = new LocationTextExtractionStrategy();

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {
                var page = pdfDocument.GetPage(i);
                string text = PdfTextExtractor.GetTextFromPage(page, strategy);
                processed.Append(text);
            }
            pdfDocument.Close();

            if(processed.Length > 0)
            {
                allLines = processed.ToString().Split("\n");
                SetFields();
            }
            else
            {
                allLines.Append("No data was scrapped! Did you provide correct Intercity Ticket? ");
            }

            return this;
        }

        private IntercityScrapper SetFields()
        {
            //Ticker number
            string ticketNumber = allLines.Where(line => line.Contains("Nr biletu : ")).FirstOrDefault();
            Model.TicketNumber = String.IsNullOrWhiteSpace(ticketNumber)
                ? "Ticket number not found"
                : ticketNumber.Split("Nr biletu : ")[1].Split(" ")[0];

            //Skip lines until "Stacja Data Godzina" line found - it will skip around 13 unnecessary lines which will reduce operation for further iterations
            IEnumerable<string> stationHeaderLine = allLines.SkipWhile(x => !x.Contains("Stacja Data Godzina"));

            //TravelerName
            string travelerLine = stationHeaderLine.Where(line => line.Contains("Podróżny")).FirstOrDefault();
            Model.TravelerName = String.IsNullOrWhiteSpace(travelerLine)
                ? "No traveler found"
                : travelerLine.Split(": ")[1];

            //StartingStation, StartDate, TrainType, TrainNumber, TravelDistance, TicketPrice
            string startingStationLine = stationHeaderLine.Skip(1).FirstOrDefault();

            if (String.IsNullOrEmpty(startingStationLine))
            {
                Model.StartingStation = "No Starting Station line found";
                Model.StartDateTime = null;
            }
            else
            {
                Match timeMatch = Regex.Match(startingStationLine, timeRegexPattern);
                Match dateMatch = Regex.Match(startingStationLine, dateRegexPattern);
                if(timeMatch.Success && dateMatch.Success)
                {
                    Model.StartDateTime = new DateTime(DateTime.Now.Year,
                                            int.Parse(dateMatch.Value.Substring(3, 2)),
                                            int.Parse(dateMatch.Value.Substring(0, 2)),
                                            int.Parse(timeMatch.Value.Substring(0, 2)),
                                            int.Parse(timeMatch.Value.Substring(3, 2)),
                                            0);

                    Model.StartingStation = startingStationLine.Substring(0, dateMatch.Index).Trim();

                    string restOfLine = startingStationLine.Substring(timeMatch.Index + 6, startingStationLine.Length - (timeMatch.Index + 6));
                    string[] restOfLineParts = restOfLine.Split(" ");

                    Model.TrainType = restOfLineParts[0];
                    Model.TrainNumber = int.Parse(restOfLineParts[1]);
                    Model.TravelDistance = int.Parse(restOfLineParts[2]);

                    string seat = restOfLineParts[3];
                    string seatNumber = seat.Substring(0, seat.Length - 1);
                    string seatType = seat.Substring(seat.Length - 1);
                    Model.Seat = seatType == "o" ? seatNumber + " okno" 
                            : seatType == "ś" ? seatNumber + " środek"
                            : seatType == "k" ? seatNumber + " korytarz"
                            : null;

                    char decimalSeparator = Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                    Model.TicketPrice = decimal.Parse(restOfLineParts[4].Replace(',', decimalSeparator));
                    Model.TicketPriceCurrency = restOfLineParts[5] == "zł" ? "PLN" : "N/A";
                }
            }

            //StartingStation, StartDate, TrainType, TrainNumber, TravelDistance, TicketPrice
            string finalStationLine = stationHeaderLine.Skip(2).FirstOrDefault();

            if (String.IsNullOrEmpty(finalStationLine))
            {
                Model.FinalStation = "No Starting Station line found";
                Model.StopDateTime = null;
            }
            else
            {
                Match timeMatch = Regex.Match(finalStationLine, timeRegexPattern);
                Match dateMatch = Regex.Match(finalStationLine, dateRegexPattern);
                if (timeMatch.Success && dateMatch.Success)
                {
                    Model.StopDateTime = new DateTime(DateTime.Now.Year,
                                            int.Parse(dateMatch.Value.Substring(3, 2)),
                                            int.Parse(dateMatch.Value.Substring(0, 2)),
                                            int.Parse(timeMatch.Value.Substring(0, 2)),
                                            int.Parse(timeMatch.Value.Substring(3, 2)),
                                            0);

                    Model.FinalStation = finalStationLine.Substring(0, dateMatch.Index).Trim();

                    string restOfLine = finalStationLine.Substring(timeMatch.Index + 6, finalStationLine.Length - (timeMatch.Index + 6));
                    string[] restOfLineParts = restOfLine.Split(" ");
                }
            }

            return this;
        }
    }
}
