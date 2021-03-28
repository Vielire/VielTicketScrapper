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
    public class IntercityScrapperTests : IntercityScrapper
    {
        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.GetOtherThanPDFFiles), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void ScrappPDF_InputIsNotPdf_ThrowsNotSupportedException(string filePath)
        {
            IntercityScrapper ic = new();

            Assert.ThrowsException<NotSupportedException>(() => {
                ic.ScrapPDF(filePath);
            });
        }

        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.GetValidFilesPaths), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void ScrapPDF_ScrappsAnyDataFromValidFile_ReturnTrue(string filePath)
        {
            IntercityScrapper ic = new();
            
            ic.ScrapPDF(filePath);
            
            Assert.IsTrue(ic.allLines.Any());
        }

        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.PurchaseDateLaterThanDepartureDate), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void GetDateTime_PurchaseDateLaterThanDepartureDate_ThrowInvalidDataException(string departureLine, string paymentDateAsString)
        {
            Assert.ThrowsException<InvalidDataException>(()=> {
                DateTime paymentDate = DateTime.Parse(paymentDateAsString);
                DateTime dt = GetDateTime(departureLine, paymentDate);
            });
        }

        [DataTestMethod]
        [DynamicData(nameof(FakeDataProvider.ValidPurchaseDate), typeof(FakeDataProvider), DynamicDataSourceType.Method)]
        public void GetDateTime_ValidPurchaseDate_ReturnTrue(string departureLine, string paymentDateAsString)
        {
            DateTime paymentDate = DateTime.Parse(paymentDateAsString);
            DateTime dt = GetDateTime(departureLine, paymentDate);

            Assert.IsTrue(dt != default);
        }
    }
}
