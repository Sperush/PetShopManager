using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;

namespace PetShop_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly PetShopDbContext _context;

        public AccountsController(PetShopDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Account>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == request.Username && a.PasswordHash == request.Password && a.IsActive == true);
            if (user == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
            }
            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}