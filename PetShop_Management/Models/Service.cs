using System;
using System.Collections.Generic;

namespace PetShop_Server.Models;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}
