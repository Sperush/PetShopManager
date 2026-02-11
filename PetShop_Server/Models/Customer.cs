using System;
using System.Collections.Generic;

namespace PetShop_Server.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
