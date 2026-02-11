using System;
using System.Collections.Generic;

namespace PetShop_Server.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int PetId { get; set; }

    public DateTime? BookingDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual Customer? Customer { get; set; }

    public virtual Pet? Pet { get; set; }
}
