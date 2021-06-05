using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.Scrappers
{
    public class Scrapper
    {
        public IEnumerable<string> allLines;

        public void ScrapPDF(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".pdf")
                throw new NotSupportedException("File type not supported.");

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
        }
    }
}
