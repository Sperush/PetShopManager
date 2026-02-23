using System.ComponentModel.DataAnnotations;

namespace PetShop_Server.Models;

public partial class Account
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
}