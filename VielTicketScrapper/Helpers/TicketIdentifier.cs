﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VielTicketScrapper.Builders.Ticket;
using VielTicketScrapper.Scrappers;

namespace VielTicketScrapper.Helpers
{
    public static class TicketIdentifier
    {
        public static TicketBuilder InstantiateTicketBuilder(Scrapper scrapper)
        {
            if (IsItIntercity(scrapper.allLines))
            {
                return new IntercityModelBuilder(scrapper.allLines);
            }
            else if (IsItPolregio(scrapper.allLines))
            {
                return new PolregioModelBuilder(scrapper.allLines);
            }
            else if (IsItKolejeSlaskie(scrapper.allLines))
            {
                return new KolejeSlaskieModelBuilder(scrapper.allLines);
            }
            else
            {
                throw new NotSupportedException($"Provided file [{scrapper.filePath}] is not supported");
            }
        }

        private static bool IsItKolejeSlaskie(IEnumerable<string> lines)
        {
            return lines.Any(l => l.ToLower().Contains("koleje śląskie"));
        }

        private static bool IsItIntercity(IEnumerable<string> lines)
        {
            return lines.Any(l => l.ToLower().Contains("intercity"));
        }

        private static bool IsItPolregio(IEnumerable<string> lines)
        {
            return lines.Any(l => l.ToLower().Contains("polregio"));
        }
    }
}
