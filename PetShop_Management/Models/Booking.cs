namespace PetShop_Management.Models;

public partial class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int PetId { get; set; }
    public DateTime? BookingDate { get; set; }
    public string? Status { get; set; } = "Chờ xử lý";
    public decimal? TotalAmount { get; set; } = 0;
    public string? PaymentStatus { get; set; } = "Chưa thanh toán";
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public decimal? AmountPaid { get; set; } = 0;
    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    public virtual Customer? Customer { get; set; }
    public virtual Pet? Pet { get; set; }
}