using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Scrappers
{
    class IntercityScrapper : IScrapper
    {
        StringBuilder processed = new StringBuilder();
        public IScrapper Scrap(string filePath)
        {
            var src = filePath;
            var pdfDocument = new PdfDocument(new PdfReader(src));
            var strategy = new LocationTextExtractionStrategy();

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {
                var page = pdfDocument.GetPage(i);
                string text = PdfTextExtractor.GetTextFromPage(page, strategy);
                processed.Append(text);
            }
            pdfDocument.Close();

            return this;
        }

        public string GetData()
        {
            return processed.ToString();
        }
    }
}
