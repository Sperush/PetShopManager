using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;

namespace PetShop_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly PetShopDbContext _context;

        public PetsController(PetShopDbContext context)
        {
            _context = context;
        }

        // GET: api/pets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pet>>> GetPets()
        {
            // Include Customer để hiển thị tên chủ
            // Include PetType để hiển thị tên loại (Chó, Mèo...)
            return await _context.Pets
                .Include(p => p.Customer)
                .Include(p => p.PetType)
                .OrderByDescending(p => p.Id) // Sắp xếp mới nhất lên đầu
                .ToListAsync();
        }

        // POST: api/pets
        [HttpPost]
        public async Task<ActionResult<Pet>> PostPet(Pet pet)
        {
            // QUAN TRỌNG: Xóa ModelState của các object quan hệ để tránh lỗi Validation "Required"
            ModelState.Remove("Customer");
            ModelState.Remove("PetType");

            // Kiểm tra xem CustomerId có tồn tại không
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == pet.CustomerId);
            if (!customerExists)
            {
                return BadRequest($"Khách hàng với ID {pet.CustomerId} không tồn tại.");
            }

            // Kiểm tra xem PetTypeId có tồn tại không
            var typeExists = await _context.PetTypes.AnyAsync(t => t.Id == pet.PetTypeId);
            if (!typeExists)
            {
                return BadRequest($"Loại thú cưng với ID {pet.PetTypeId} không tồn tại.");
            }

            // Set null để EF Core chỉ dùng ID (Foreign Key) để insert, không cố insert object con
            pet.Customer = null;
            pet.PetType = null;
            pet.Bookings = null; // Tránh lỗi nếu lỡ gửi kèm list bookings rỗng

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPets", new { id = pet.Id }, pet);
        }

        // PUT: api/pets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, Pet pet)
        {
            if (id != pet.Id) return BadRequest();

            // QUAN TRỌNG: Cũng cần xóa ModelState khi update
            ModelState.Remove("Customer");
            ModelState.Remove("PetType");

            // Tách Pet ra khỏi Context để tránh lỗi Tracking nếu lỡ load trước đó
            var existingPet = await _context.Pets.FindAsync(id);
            if (existingPet == null) return NotFound();

            // Cập nhật từng trường thủ công để an toàn hơn (hoặc dùng Entry Modified)
            existingPet.Name = pet.Name;
            existingPet.Weight = pet.Weight;
            existingPet.CustomerId = pet.CustomerId; // Cập nhật chủ mới (nếu đổi)
            existingPet.PetTypeId = pet.PetTypeId;   // Cập nhật loại mới (nếu đổi)

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pets.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/pets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            // Kiểm tra xem Pet này có Booking nào không
            var hasBookings = await _context.Bookings.AnyAsync(b => b.PetId == id);
            if (hasBookings)
            {
                // Tùy chọn: Chặn xóa hoặc Xóa luôn cả Booking
                // Ở đây mình chọn chặn xóa để an toàn dữ liệu
                return BadRequest("Không thể xóa thú cưng này vì đã có lịch sử đặt lịch.");
            }

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}