using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;

namespace PetShop_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetTypesController : ControllerBase
    {
        private readonly PetShopDbContext _context;

        public PetTypesController(PetShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetType>>> GetPetTypes()
        {
            return await _context.PetTypes.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<PetType>> PostPetType(PetType petType)
        {
            _context.PetTypes.Add(petType);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPetTypes", new { id = petType.Id }, petType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPetType(int id, PetType petType)
        {
            if (id != petType.Id) return BadRequest();
            _context.Entry(petType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetType(int id)
        {
            var petType = await _context.PetTypes.FindAsync(id);
            if (petType == null) return NotFound();

            // Kiểm tra xem có thú cưng nào thuộc loại này không trước khi xóa
            var hasPets = await _context.Pets.AnyAsync(p => p.PetTypeId == id);
            if (hasPets)
            {
                return BadRequest("Không thể xóa loại này vì đang có thú cưng thuộc loại đó.");
            }

            _context.PetTypes.Remove(petType);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}