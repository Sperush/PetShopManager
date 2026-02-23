using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;
using PetShop_Server.DTOs;

namespace PetShop_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly PetShopDbContext _context;

        public OrdersController(PetShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.Include(o => o.Customer).OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderRequest request)
        {
            var order = new Order
            {
                CustomerId = request.CustomerId,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Đã thanh toán",
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                OrderDetails = new List<OrderDetail>()
            };
            decimal total = 0;
            foreach (var item in request.CartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPriceAtPurchase = product.Price
                    });
                    total += (product.Price * item.Quantity);
                    if (product.Stock >= item.Quantity)
                    {
                        product.Stock -= item.Quantity;
                    }
                    else
                    {
                        return BadRequest($"Sản phẩm {product.Name} không đủ số lượng trong kho!");
                    }
                }
            }
            order.TotalAmount = total;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(order);
        }
    }
}