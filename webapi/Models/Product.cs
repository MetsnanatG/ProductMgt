namespace webapi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }   // Total available stock of SIM cards
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }

    public string UserId { get; set; } // Add this property

    // Navigation property for the user
    public virtual User User { get; set; }
}
