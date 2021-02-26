using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VielTicketScrapper.Models.Enums
{
    public enum ExportFileType
    {
        Text,
        ICal
    }

    static class ExportFileTypeMethods
    {
        public static String FriendlyName(this ExportFileType fileType)
        {
            switch (fileType)
            {
                case ExportFileType.Text:
                    return "text";
                case ExportFileType.ICal:
                    return "iCalendar";
                default:
                    return Enum.GetName(typeof(ExportFileType), fileType);
            }
        }
    }
}
