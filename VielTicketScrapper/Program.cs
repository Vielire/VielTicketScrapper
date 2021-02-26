using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VielTicketScrapper.Models.Enums;

namespace VielTicketScrapper
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Command("intercity", "Scrap the data from Intercity ticket!")
                {
                    new Argument<string>("filepath"),
                    new Option<ExportFileType>("--to", "ical/text"),
                    new Option<bool>("--verbose")
                }.WithHandler(nameof(Intercity))
            };

            return await cmd.InvokeAsync(args);
        }
        private static void Intercity(string filepath, ExportFileType to, bool verbose, IConsole console)
        {
            string[] filepathParts = filepath.Split(Path.DirectorySeparatorChar);
            string fileName = filepathParts[filepathParts.Length-1];
            if (verbose)
                console.Out.WriteLine($"About to scrap data from the '{fileName}' file...");

            console.Out.WriteLine($"{fileName} scrapped to {to} fle!");

            if (verbose)
                console.Out.WriteLine($"All done!");
        }

        private static Command WithHandler(this Command command, string methodName)
        {
            var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var handler = CommandHandler.Create(method);
            command.Handler = handler;
            return command;
        }
    }
}
