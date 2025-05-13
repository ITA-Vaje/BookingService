using BookingService.Data;
using BookingService.MessagingServices;
using BookingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingDbContext _context;
        private readonly ArtemisPublisher _mqPublisher;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BookingDbContext context, ArtemisPublisher mqPublisher, ILogger<BookingController> logger)
        {
            _context = context;
            _mqPublisher = mqPublisher;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var message = $"Created new booking with id: {booking.Id}";

            try
            {
                _mqPublisher.SendBookingCreatedMessage(booking);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send create message to ActiveMQ");
            }

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }

        // PUT: api/booking/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, Booking updatedBooking)
        {
            if (id != updatedBooking.Id)
                return BadRequest();

            _context.Entry(updatedBooking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                var message = $"Updated booking with id: {updatedBooking.Id}";
                _mqPublisher.SendBookingUpdatedMessage(updatedBooking);
                _logger.LogInformation(message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(b => b.Id == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send update message to ActiveMQ");
            }

            return NoContent();
        }

        // DELETE: api/booking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            var message = $"Deleted booking with ID {booking.Id}";

            try
            {
                _mqPublisher.SendBookingDeletedMessage(booking);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send delete message to ActiveMQ");
            }

            return NoContent();
        }
    }
}
