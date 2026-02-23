namespace PetShop_Management.Models;

public partial class PetType
{
    public int Id { get; set; }
    public string TypeName { get; set; } = null!;
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
