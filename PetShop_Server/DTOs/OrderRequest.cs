namespace PetShop_Server.DTOs
{
    public class OrderRequest
    {
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}