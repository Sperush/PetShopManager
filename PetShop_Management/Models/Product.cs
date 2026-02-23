namespace PetShop_Management.Models;

public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int? Stock { get; set; } = 0;
    public bool IsDeleted { get; set; } = false;
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}