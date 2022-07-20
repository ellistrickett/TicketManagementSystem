using System;
using System.Configuration;
using System.IO;
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

        public int CreateTicket(string title, Priority priority, string assignedTo, string description, DateTime createdAt, bool isPayingCustomer)
        {
            // Check if title or description are null or empty then throw exception
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = null;
            var userRepository = new UserRepository();
            if (assignedTo != null)
            {
                user = userRepository.GetUser(assignedTo);
            }

            var priorityyRaised = false;
            if (createdAt < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                if (priority == Priority.Low)
                {
                    priority = Priority.Medium;
                    priorityyRaised = true;
                }
                else if (priority == Priority.Medium)
                {
                    priority = Priority.High;
                    priorityyRaised = true;
                }
            }

            // If the title contains some special worrds and the priority has not yet been raised, raise it here.
            if ((title.Contains("Crash") || title.Contains("Important") || title.Contains("Failure")) && !priorityyRaised)
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

            double price = 0;
            User accountManager = null;
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

            // Create the tickket
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

        public void AssignTicket(int id, string username)
        {
            User user = null;
            var ur = new UserRepository();
            if (username != null)
            {
                user = ur.GetUser(username);
            }

            var ticket = ticketRepository.GetTicket(id);

            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }

            ticket.AssignedUser = user;

            ticketRepository.UpdateTicket(ticket);
        }

        private void WriteTicketToFile(Ticket ticket)
        {
            var ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
        }
    }

    public enum Priority
    {
        High,
        Medium,
        Low
    }
}
