using BookingService.Controllers;
using BookingService.Data;
using BookingService.MessagingServices;
using BookingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookingService.Tests
{
    public class BookingControllerTests
    {
        private readonly Mock<ILogger<BookingController>> _mockLogger;
        private readonly DbContextOptions<BookingDbContext> _dbOptions;

        public BookingControllerTests()
        {
            // Setup in-memory database for testing
            _dbOptions = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Setup logger mock
            _mockLogger = new Mock<ILogger<BookingController>>();
        }

        // Test 1: GetBookings should return all bookings
        [Fact]
        public async Task GetBookings_ReturnsAllBookings()
        {
            // Arrange
            using var context = new BookingDbContext(_dbOptions);
            SeedDatabase(context);

            // Create a null publisher to bypass the messaging functionality
            ArtemisPublisher nullPublisher = null;
            var controller = new BookingController(context, nullPublisher, _mockLogger.Object);

            // Act
            var result = await controller.GetBookings();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Booking>>>(result);
            var bookings = Assert.IsAssignableFrom<IEnumerable<Booking>>(actionResult.Value);
            Assert.Equal(2, bookings.Count());
        }

        // Test 2: GetBooking should return a specific booking
        [Fact]
        public async Task GetBooking_WithValidId_ReturnsBooking()
        {
            // Arrange
            using var context = new BookingDbContext(_dbOptions);
            SeedDatabase(context);

            // Create a null publisher to bypass the messaging functionality
            ArtemisPublisher nullPublisher = null;
            var controller = new BookingController(context, nullPublisher, _mockLogger.Object);
            int bookingId = 1;

            // Act
            var result = await controller.GetBooking(bookingId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Booking>>(result);
            var booking = Assert.IsType<Booking>(actionResult.Value);
            Assert.Equal(bookingId, booking.Id);
            Assert.Equal("CAR123", booking.VehicleId);
        }

        // Test 3: CreateBooking should add a new booking
        [Fact]
        public async Task CreateBooking_AddsNewBooking_ReturnsCreatedAtAction()
        {
            // Arrange
            using var context = new BookingDbContext(_dbOptions);

            // Create a null publisher to bypass the messaging functionality
            ArtemisPublisher nullPublisher = null;
            var controller = new BookingController(context, nullPublisher, _mockLogger.Object);

            var newBooking = new Booking
            {
                VehicleId = "CAR789",
                UserId = "user3",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(7),
                TotalPrice = 150.00m,
                Status = "Pending"
            };

            // Act
            var result = await controller.CreateBooking(newBooking);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Booking>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<Booking>(createdAtActionResult.Value);

            Assert.Equal(newBooking.VehicleId, returnValue.VehicleId);
            Assert.Equal(1, returnValue.Id); // First booking should have ID 1
        }

        // Test 4: DeleteBooking should remove a booking
        [Fact]
        public async Task DeleteBooking_WithValidId_RemovesBookingAndReturnsNoContent()
        {
            // Arrange
            using var context = new BookingDbContext(_dbOptions);
            SeedDatabase(context);

            // Create a null publisher to bypass the messaging functionality
            ArtemisPublisher nullPublisher = null;
            var controller = new BookingController(context, nullPublisher, _mockLogger.Object);
            int bookingId = 1;

            // Act
            var result = await controller.DeleteBooking(bookingId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify booking was deleted
            var bookingExists = await context.Bookings.AnyAsync(b => b.Id == bookingId);
            Assert.False(bookingExists);
        }

        // Helper method to seed the database
        private void SeedDatabase(BookingDbContext context)
        {
            context.Bookings.AddRange(
                new Booking
                {
                    Id = 1,
                    VehicleId = "CAR123",
                    UserId = "user1",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(2),
                    TotalPrice = 100.00m,
                    Status = "Confirmed"
                },
                new Booking
                {
                    Id = 2,
                    VehicleId = "CAR456",
                    UserId = "user2",
                    StartDate = DateTime.Today.AddDays(3),
                    EndDate = DateTime.Today.AddDays(4),
                    TotalPrice = 80.00m,
                    Status = "Pending"
                }
            );
            context.SaveChanges();
        }
    }
}