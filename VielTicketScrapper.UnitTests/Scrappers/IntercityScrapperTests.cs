using Microsoft.VisualStudio.TestTools.UnitTesting;
using VielTicketScrapper.Scrappers;
using System;
using System.IO;
using System.Linq;
using iText.Signatures;

namespace VielTicketScrapper.UnitTests.Scrappers
{
    [TestClass]
    public class IntercityScrapperTests : IntercityScrapper
    {
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ScrappPDF_InputIsNotPdf_ThrowsNotSupportedException()
        {
            IntercityScrapper ic = new();
            string parent = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            ic.ScrapPDF(Path.Combine(parent, "VielTicketScrapper", "ExampleFiles", "Not_a_PDF_file.txt"));
        }

        [TestMethod]
        public void ScrapPDF_ScrappsAnyData_ReturnTrue()
        {
            IntercityScrapper ic = new();
            string parent = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            ic.ScrapPDF(Path.Combine(parent, "VielTicketScrapper", "ExampleFiles", "Example_Intercity.pdf"));
            Assert.IsTrue(ic.allLines.Any());
        }

        [DataTestMethod]
        [DataRow("Katowice 30.12 13:00...", "2021.12.31")]
        [DataRow("Katowice 01.02 13:00...", "2021.02.03")]
        [DataRow("Będzin 28.02 13:00...", "2021.03.03")]
        [DataRow("Bydgoszcz Główna 28.02 13:00...", "2021.04.30")]
        public void GetDateTime_PurchaseDateLaterThanDepartureDate_ThrowInvalidDataException(string departureLine, string paymentDateAsString)
        {
            Assert.ThrowsException<InvalidDataException>(()=> {
                DateTime paymentDate = DateTime.Parse(paymentDateAsString);
                DateTime dt = IntercityScrapper.GetDateTime(departureLine, paymentDate);
            });
        }

        [DataTestMethod]
        [DataRow("Katowice 03.02 13:00...", "2021.02.03")]
        [DataRow("Katowice 28.02 13:00...", "2021.02.02")]
        [DataRow("Będzin 05.01 13:00...", "2020.12.30")]
        [DataRow("Bydgoszcz Główna 02.01 13:00...", "2021.12.05")]
        public void GetDateTime_ValidPurchaseDate_ReturnTrue(string departureLine, string paymentDateAsString)
        {
            DateTime paymentDate = DateTime.Parse(paymentDateAsString);
            DateTime dt = IntercityScrapper.GetDateTime(departureLine, paymentDate);

            Assert.IsTrue(dt != default);
        }
    }
}
