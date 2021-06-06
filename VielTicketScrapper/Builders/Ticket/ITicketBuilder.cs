using VielTicketScrapper.Models.Tickets;

namespace VielTicketScrapper.Builders.Ticket
{
    public interface ITicketBuilder
    {
        Models.Tickets.Ticket Build();
    }
}