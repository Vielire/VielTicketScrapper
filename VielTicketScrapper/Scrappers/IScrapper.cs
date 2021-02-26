using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Scrappers
{
    interface IScrapper
    {
        IScrapper Scrap(string filePath);
        string GetData();
    }
}
