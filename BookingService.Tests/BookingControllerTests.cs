using BookingService.Controllers;
using BookingService.Data;
using BookingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BookingService.Tests
{
    public class BookingControllerTests
    {
        private readonly BookingDbContext _context;
        private readonly Mock<ArtemisPublisher> _mqMock = new(MockBehavior.Strict);
        private readonly ILogger<BookingController> _logger = Mock.Of<ILogger<BookingController>>();
        private readonly BookingController _controller;

        public BookingControllerTests()
        {
            var options = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: "BookingDbTest")
                .Options;


            _context = new BookingDbContext(options);
            _controller = new BookingController(_context, _mqMock.Object, _logger);
        }

        [Fact]
        public async Task CreateBooking_Returns_CreatedAtActionResult()
        {
            // Arrange
            var booking = new Booking
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Status = "Confirmed",
                TotalPrice = 150.0m,
                UserId = "user-123",
                VehicleId = "vehicle-123"
            };

            _mqMock.Setup(m => m.SendBookingCreatedMessage(It.IsAny<Booking>()));

            // Act
            var result = await _controller.CreateBooking(booking);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedBooking = Assert.IsType<Booking>(createdResult.Value);
            Assert.Equal(booking.UserId, returnedBooking.UserId);
        }

        [Fact]
        public async Task GetBookings_Returns_AllBookings()
        {
            // Arrange
            _context.Bookings.Add(new Booking
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2),
                Status = "Pending",
                TotalPrice = 99.99m,
                UserId = "testuser",
                VehicleId = "veh1"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetBookings();

            // Assert
            var bookings = Assert.IsType<List<Booking>>(result.Value);
            Assert.NotEmpty(bookings);
        }
    }
}
