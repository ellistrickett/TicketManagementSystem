using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TicketManagementSystem
{
    public class TicketService
    {
        ITicketRepository ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            this.ticketRepository = ticketRepository;
        }

        public int CreateTicket(string title, Priority priority, string assignedTo, string description, DateTime createdAt, bool isPayingCustomer, DateTime? utcNow = null)
        {
            // Check if title or description are null or empty then throw exception
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = GetUser(assignedTo);

            priority = SetPriority(title, priority, createdAt, utcNow);

            double price = SetPrice(priority, isPayingCustomer, out User accountManager);

            // Create the ticket
            var ticket = new Ticket()
            {
                Title = title,
                AssignedUser = user,
                Priority = priority,
                Description = description,
                Created = createdAt,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = ticketRepository.CreateTicket(ticket);

            // Return the id
            return id;
        }

        public void AssignTicket(int ticketId, string username)
        {
            var user = GetUser(username);

            var ticket = ticketRepository.GetTicket(ticketId);

            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + ticketId);
            }

            ticket.AssignedUser = user;

            ticketRepository.UpdateTicket(ticket);
        }

        private static User GetUser(string assignedTo)
        {
            User user = null;
            var userRepository = new UserRepository();
            if (assignedTo != null)
            {
                user = userRepository.GetUser(assignedTo);
            }

            return user;
        }

        private static Priority SetPriority(string title, Priority priority, DateTime createdAt, DateTime? utcNow)
        {
            utcNow ??= DateTime.UtcNow;

            if (createdAt < utcNow - TimeSpan.FromHours(1) ||
                new[] { "Crash", "Important", "Failure" }.Any(c => title.Contains(c)))
            {
                if (priority == Priority.Low)
                {
                    priority = Priority.Medium;
                }
                else if (priority == Priority.Medium)
                {
                    priority = Priority.High;
                }
            }

            return priority;
        }
        private static double SetPrice(Priority priority, bool isPayingCustomer, out User accountManager)
        {
            double price = 0;
            accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                accountManager = new UserRepository().GetAccountManager();
                if (priority == Priority.High)
                {
                    price = 100;
                }
                else
                {
                    price = 50;
                }
            }
            return price;
        }

    }
}
