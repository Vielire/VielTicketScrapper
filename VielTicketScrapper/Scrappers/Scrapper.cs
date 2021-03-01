using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.Scrappers
{
    abstract class Scrapper
    {
        public IEnumerable<string> allLines;

        public virtual Scrapper ScrapPDF(string filePath)
        {
            StringBuilder sb = new();
            var pdfDocument = new PdfDocument(new PdfReader(filePath));
            var strategy = new LocationTextExtractionStrategy();

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {
                var page = pdfDocument.GetPage(i);
                string text = PdfTextExtractor.GetTextFromPage(page, strategy);
                sb.Append(text);
            }

            if (sb.Length > 0)
            {
                allLines = sb.ToString().Split("\n");
            }

            pdfDocument.Close();
            return this;
        }
        
        public abstract Ticket ParseToTicket();
    }
}
