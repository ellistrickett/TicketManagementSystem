using System;
using Moq;
using NUnit.Framework;

namespace TicketManagementSystem.Test
{
    public class Tests
    {
        private TicketService ticketService;
        private Mock<ITicketRepository> ticketRepositoryMock;

        private string title;
        private Priority priority;
        private string assignedTo;
        private string description;
        private DateTime createdAt;

        [SetUp]
        public void Setup()
        {
            ticketRepositoryMock = new Mock<ITicketRepository>();
            ticketService = new TicketService(ticketRepositoryMock.Object);

            title = "MyTicket";
            priority = Priority.High;
            assignedTo = "jsmith";
            description = "This is a ticket";
            createdAt = DateTime.Now;
        }

        [Test]
        public void WhenTitleIsNull_ThenThrowException()
        {
            Assert.That(() => ticketService.CreateTicket(null, priority, assignedTo, description, createdAt, false), Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title or description were null"));
        }

        [Test]
        public void WhenTitleIsEmptyString_ThenThrowException()
        {
            Assert.That(() => ticketService.CreateTicket("", priority, assignedTo, description, createdAt, false), Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title or description were null"));
        }

        [Test]
        public void WhenDescriptionIsIsNull_ThenThrowException()
        {
            Assert.That(() => ticketService.CreateTicket(title, priority, assignedTo, null, createdAt, false), Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title or description were null"));
        }

        [Test]
        public void WhenDescriptionIsEmptyString_ThenThrowException()
        {
            Assert.That(() => ticketService.CreateTicket(title, priority, assignedTo, "", createdAt, false), Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title or description were null"));
        }

        [Test]
        public void WhenTicketIsAssignedToUnkcreatedAtnUser_ThenThrowException()
        {
            Assert.That(() => ticketService.CreateTicket(title, priority, "jim", description, createdAt, false), Throws.InstanceOf<UnknownUserException>().With.Message.EqualTo("User jim not found"));
        }

        [Test]
        public void WhenCreatedMoreThanAnHourAgo_ThenRaisePriority()
        {
            var utcNow = new DateTime(2022, 07, 20, 19, 41, 00, DateTimeKind.Utc);

            ticketService.CreateTicket(title, Priority.Medium, assignedTo, description, new DateTime(2022, 07, 20, 18, 40, 00, DateTimeKind.Utc), false, utcNow);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Priority == Priority.High)));
        }

        [Test]
        public void WhenTitleContainsKeyWord_ThenRaisePriority()
        {
            ticketService.CreateTicket("System Crash", Priority.Medium, "jsmith", "high priority ticket", DateTime.Now, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Priority == Priority.High)));
        }

        [Test]
        public void CreateTicketSuccessfully()
        {
            ticketService.CreateTicket(title, priority, assignedTo, description, createdAt, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title && 
                t.Priority == Priority.High && 
                t.Description == description &&
                t.AssignedUser.Username == assignedTo && 
                t.Created == createdAt)));
        }
    }
}