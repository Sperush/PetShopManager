namespace PetShop_Server.Models;

public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsDeleted { get; set; } = false;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>(); // Thêm dòng này
}