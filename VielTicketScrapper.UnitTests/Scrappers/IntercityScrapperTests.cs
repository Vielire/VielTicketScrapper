using Microsoft.VisualStudio.TestTools.UnitTesting;
using VielTicketScrapper.Scrappers;
using System;
using System.IO;
using System.Linq;
using iText.Signatures;
using VielTicketScrapper.UnitTests.FakeData;

namespace VielTicketScrapper.UnitTests.Scrappers
{
    [TestClass]
    public class IntercityScrapperTests : Scrapper
    {
        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.GetOtherThanPDFFiles), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void ScrappPDF_InputIsNotPdf_ThrowsNotSupportedException(string filePath)
        {
            Scrapper scrapper = new();

            Assert.ThrowsException<NotSupportedException>((Action)(() => {
                scrapper.Scrap((string)filePath);
            }));
        }

        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.GetValidFilesPaths), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void ScrapPDF_ScrappsAnyDataFromValidFile_ReturnTrue(string filePath)
        {
            Scrapper scrapper = new();

            scrapper.Scrap(filePath);
            
            Assert.IsTrue(scrapper.allLines.Any());
        }
    }
}
