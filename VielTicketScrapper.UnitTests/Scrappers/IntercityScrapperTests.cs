using Microsoft.VisualStudio.TestTools.UnitTesting;
using VielTicketScrapper.Scrappers;
using System;
using System.IO;
using System.Linq;

namespace VielTicketScrapper.UnitTests.Scrappers
{
    [TestClass]
    public class IntercityScrapperTests
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

    }
}
