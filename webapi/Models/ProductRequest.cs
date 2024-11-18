using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Models;

public class ProductRequest
{
     public int Id { get; set; }

    
    public string? RequestedBy { get; set; }  // The user who made the request
    public string? Status { get; set; }       // Initiated, Approved, Rejected
    public DateTime RequestDate { get; set; }
    public int Quantity { get; set; }        // Quantity requested
    public int ProductId { get; set; }       // Reference to Product entity (optional)
    public Product? product { get; set; }     // Navigation property to the Product entity
    public string RequestedByUserId { get; set; }
    public List<ApprovalHistory> ApprovalHistory { get; set; } // List of approval history
}
