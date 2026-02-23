using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;
using PetShop_Server.DTOs;

namespace PetShop_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly PetShopDbContext _context;

        public BookingsController(PetShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.Include(b => b.Customer).Include(b => b.Pet).OrderByDescending(b => b.BookingDate).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Customer).Include(b => b.Pet).Include(b => b.BookingDetails).ThenInclude(bd => bd.Service).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }
            return booking;
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(BookingRequest request)
        {
            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                PetId = request.PetId,
                BookingDate = request.BookingDate,
                Status = "Chờ xử lý",
                BookingDetails = new List<BookingDetail>()
            };
            decimal total = 0;
            foreach (var serviceId in request.ServiceIds)
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service != null)
                {
                    booking.BookingDetails.Add(new BookingDetail
                    {
                        ServiceId = serviceId,
                        PriceAtBooking = service.Price
                    });
                    total += service.Price;
                }
            }
            booking.TotalAmount = total;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, Booking booking)
        {
            if (id != booking.Id) return BadRequest();
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            var details = _context.BookingDetails.Where(d => d.BookingId == id);
            _context.BookingDetails.RemoveRange(details);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}