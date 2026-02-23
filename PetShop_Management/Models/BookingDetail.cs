namespace PetShop_Management.Models;

public partial class BookingDetail
{
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public decimal? PriceAtBooking { get; set; }
    public virtual Booking Booking { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
