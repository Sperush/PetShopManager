namespace PetShop_Management.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtPurchase { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}