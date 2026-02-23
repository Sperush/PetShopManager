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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pet>>> GetPets()
        {
            return await _context.Pets.Include(p => p.Customer).Include(p => p.PetType).OrderByDescending(p => p.Id).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Pet>> PostPet(Pet pet)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("PetType");
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == pet.CustomerId);
            if (!customerExists)
            {
                return BadRequest($"Khách hàng với ID {pet.CustomerId} không tồn tại.");
            }
            var typeExists = await _context.PetTypes.AnyAsync(t => t.Id == pet.PetTypeId);
            if (!typeExists)
            {
                return BadRequest($"Loại thú cưng với ID {pet.PetTypeId} không tồn tại.");
            }
            pet.Customer = null;
            pet.PetType = null;
            pet.Bookings = new List<Booking>();
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPets", new { id = pet.Id }, pet);
        }

        // PUT: api/pets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, Pet pet)
        {
            if (id != pet.Id) return BadRequest();
            ModelState.Remove("Customer");
            ModelState.Remove("PetType");
            var existingPet = await _context.Pets.FindAsync(id);
            if (existingPet == null) return NotFound();
            existingPet.Name = pet.Name;
            existingPet.Weight = pet.Weight;
            existingPet.CustomerId = pet.CustomerId;
            existingPet.PetTypeId = pet.PetTypeId;
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();
            var hasBookings = await _context.Bookings.AnyAsync(b => b.PetId == id);
            if (hasBookings)
            {
                return BadRequest("Không thể xóa thú cưng này vì đã có lịch sử đặt lịch.");
            }
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}