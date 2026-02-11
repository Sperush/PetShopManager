namespace PetShop_Server.DTOs
{
    public class BookingRequest
    {
        public int CustomerId { get; set; }
        public int PetId { get; set; }
        public DateTime BookingDate { get; set; }

        // Danh sách ID các dịch vụ khách chọn (Ví dụ: [1, 3])
        public List<int> ServiceIds { get; set; } = new List<int>();
    }
}