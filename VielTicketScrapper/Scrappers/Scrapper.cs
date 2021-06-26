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
        public string filePath;

        public void Scrap(string filePath)
        {
            this.filePath = filePath;
            if (Path.GetExtension(this.filePath).ToLower() != ".pdf")
                throw new NotSupportedException("File type not supported.");

            PdfDocument pdfDocument = new(new PdfReader(this.filePath));

            string plainText = ReadPlainText(pdfDocument);
            if (plainText.Length > 0)
            {
                allLines = plainText.Split("\n");
            }

            pdfDocument.Close();
        }

        public void Scrap(Stream stream)
        {
            PdfDocument pdfDocument = new(new PdfReader(stream));

            string plainText = ReadPlainText(pdfDocument);
            if (plainText.Length > 0)
            {
                allLines = plainText.Split("\n");
            }

            pdfDocument.Close();
        }

        #region private methods
        private string ReadPlainText(PdfDocument doc)
        {
            StringBuilder sb = new();

            for (int i = 1; i <= doc.GetNumberOfPages(); ++i)
            {
                var page = doc.GetPage(i);
                string text = PdfTextExtractor.GetTextFromPage(page, new LocationTextExtractionStrategy());
                sb.Append(text);
            }

            return sb.ToString();
        }
        #endregion
    }
}
