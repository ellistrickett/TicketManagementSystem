using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManagementSystem
{
    public class UserRepository
    {
        private readonly List<User> users = new()
        {
            new() { FirstName = "John", LastName = "Smith", Username = "jsmith" },
            new() { FirstName = "Sarah", LastName = "Berg", Username = "sberg" }
        };

        public User GetUser(string username)
        {
            var user = users.FirstOrDefault(a => a.Username == username);

            if (user == null)
            {
                throw new UnknownUserException("User " + username + " not found");
            }

            return user;
        }

        public User GetAccountManager()
        {
            // Assume this method does not need to change.
            return GetUser("sberg");
        }

        // Open-Closed principle
        public void AddUser(User user)
        {
            users.Add(user);
        }
    }
}
