namespace PetShop_Management.DTOs
{
    public class OrderRequest
    {
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
    }
}