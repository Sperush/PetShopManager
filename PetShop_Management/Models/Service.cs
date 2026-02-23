namespace PetShop_Management.Models;

public partial class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; } = false;
    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}