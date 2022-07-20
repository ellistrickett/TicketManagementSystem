using System;

namespace TicketManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ticket Service Test Harness");

            var service = new TicketService(new TicketRepository());

            Console.WriteLine("Creating new ticket.");
            var ticketId = service.CreateTicket(
                "System Crash",
                Priority.Medium,
                "jsmith",
                "The system crashed when user performed a search",
                DateTime.UtcNow,
                true);

            Console.WriteLine("Assigning ticket to sberg");
            service.AssignTicket(ticketId, "sberg");

            Console.WriteLine("Done");
        }
    }
}
