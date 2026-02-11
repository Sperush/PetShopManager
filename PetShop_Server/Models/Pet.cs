using System;
using System.Collections.Generic;

namespace PetShop_Server.Models;

public partial class Pet
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int PetTypeId { get; set; }

    public string Name { get; set; } = null!;

    public double? Weight { get; set; }

    public virtual ICollection<Booking>? Bookings { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual PetType? PetType { get; set; }
}
