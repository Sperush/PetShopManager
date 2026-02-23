namespace PetShop_Server.Models;

public partial class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime? OrderDate { get; set; } = DateTime.Now;
    public decimal? TotalAmount { get; set; } = 0;
    public string? PaymentStatus { get; set; } = "Chưa thanh toán";
    public string? PaymentMethod { get; set; } = "Tiền mặt";
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}