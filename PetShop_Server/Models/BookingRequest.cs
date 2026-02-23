namespace PetShop_Server.DTOs
{
    public class BookingRequest
    {
        public int CustomerId { get; set; }
        public int PetId { get; set; }
        public DateTime BookingDate { get; set; }
        public List<int> ServiceIds { get; set; } = new List<int>();
    }
}