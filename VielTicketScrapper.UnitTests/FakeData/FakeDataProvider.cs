using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.UnitTests.FakeData
{
    public static class FakeDataProvider
    {
        private static string GetProjectDirectory()
        {
            return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
        }

        public static IEnumerable<object[]> PurchaseDateLaterThanDepartureDate()
        {
            yield return new object[] { "Katowice 30.12 13:00...", "2021.12.31" };
            yield return new object[] { "Katowice 01.02 13:00...", "2021.02.03" };
            yield return new object[] { "Będzin 28.02 13:00...", "2021.03.03" };
            yield return new object[] { "Bydgoszcz Główna 28.02 13:00...", "2021.04.30" };
        }

        public static IEnumerable<object[]> ValidPurchaseDate()
        {
            yield return new object[] { "Katowice 03.02 13:00...", "2021.02.03" };
            yield return new object[] { "Katowice 28.02 13:00...", "2021.02.02" };
            yield return new object[] { "Będzin 05.01 13:00...", "2020.12.30" };
            yield return new object[] { "Bydgoszcz Główna 02.01 13:00...", "2021.12.05" };
        }

        public static IEnumerable<object[]> GetValidFilesPaths()
        {
            yield return new object[] { Path.Combine(GetProjectDirectory(), "VielTicketScrapper", "ExampleFiles", "Example_Intercity.pdf") };
        }

        public static IEnumerable<object[]> GetOtherThanPDFFiles()
        {
            yield return new object[] { Path.Combine(GetProjectDirectory(), "VielTicketScrapper", "ExampleFiles", "Not_a_PDF_file.txt") };
        }
    }
}
