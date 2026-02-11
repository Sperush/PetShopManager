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

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            // SỬA: _context.Bookings (số nhiều) thay vì _context.Booking
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Pet)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        // THÊM MỚI: API lấy chi tiết cho trang Booking Details
        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Pet)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Service) // Lấy luôn tên dịch vụ
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // POST: api/bookings
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(BookingRequest request)
        {
            // Tạo đối tượng Booking
            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                PetId = request.PetId,
                BookingDate = request.BookingDate,
                Status = "Chờ xử lý",
                BookingDetails = new List<BookingDetail>() // Khởi tạo list chi tiết
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
            await _context.SaveChangesAsync(); // Lưu một lần duy nhất cả đơn hàng và chi tiết

            return Ok(booking);
        }

        // Cần thêm hàm PUT (Cập nhật trạng thái) và DELETE nếu muốn nút xóa hoạt động
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

            // Xóa chi tiết trước (nếu không setup Cascade Delete trong DB)
            var details = _context.BookingDetails.Where(d => d.BookingId == id);
            _context.BookingDetails.RemoveRange(details);

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}