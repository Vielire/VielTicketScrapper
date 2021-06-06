using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Builders.Ticket
{
    public abstract class TicketBuilder
    {
        protected string TimeRegexPattern = @"[0-2]\d[:][0-5]\d";
        protected string DateRegexPattern = @"[0-3]\d[.][0-1]\d";

        protected readonly IEnumerable<string> allLines;
        protected readonly List<string> allLinesAsList;

        public virtual string NotSupportedExMessage => "The file is not supported.";
        public abstract Models.Tickets.Ticket Build();

        public TicketBuilder(IEnumerable<string> allLines)
        {
            this.allLines = allLines;
            this.allLinesAsList = allLines.ToList();
        }
    }
}
