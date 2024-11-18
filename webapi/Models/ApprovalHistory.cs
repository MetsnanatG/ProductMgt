namespace webapi.Models;

public class ApprovalHistory
{
    public int Id { get; set; }
    public int ProductRequestId { get; set; }    // Foreign key to ProductRequest
    public ProductRequest ProductRequest { get; set; } // Navigation property to ProductRequest
    public string ApprovedBy { get; set; }
    public string RejectedBy { get; set; } // User who rejected the request
    public string Action { get; set; }           // Approved or Rejected
    public DateTime ActionDate { get; set; }

}




