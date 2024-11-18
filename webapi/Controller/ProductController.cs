namespace webapi.Controller;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using webapi.Models;
using webapi.Service;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly Productservice _Productservice;
    private readonly UserManager<User> _userManager;

    public ProductController(Productservice Productservice)
    {
        _Productservice = Productservice;
    }

    [Authorize(Roles = "Tester")] // Only testers can create requests
    [HttpPost("request")]
    public async Task<IActionResult> RequestProduct([FromBody] ProductRequestModel model)
    {
        var result = await _Productservice.CreateProductRequest(quantity: model.Quantity, requestedByUserId: model.RequestedBy, ProductId: model.ProductId);
        return Ok(result);
    }

    [HttpPost("approve/{requestId}")]
    [Authorize(Roles = "TestLead,Manager")] // Only Test Leads and Managers can approve requests
    public async Task<IActionResult> ApproveProductRequest(int requestId, [FromBody] ApprovalModel model)
    {
        var result = await _Productservice.ProcessProductRequest(requestId, "Approved", model.ApprovedBy , model.RejectedBy);
        if (!result)
            return BadRequest("Unable to approve the request.");

        return Ok("Request approved successfully.");
    }

    [HttpPost("reject/{requestId}")]
    [Authorize(Roles = "TestLead,Manager")] // Only Test Leads and Managers can reject requests
    public async Task<IActionResult> RejectProductRequest(int requestId, [FromBody] ApprovalModel model)
    {
        var result = await _Productservice.ProcessProductRequest(requestId, "Rejected", model.ApprovedBy , model.RejectedBy);
        if (!result)
            return BadRequest("Unable to reject the request.");

        return Ok("Request rejected successfully.");
    }

    [HttpGet("history/{requestId}")]
    public async Task<IActionResult> GetApprovalHistory(int requestId)
    {
        var history = await _Productservice.GetApprovalHistory(requestId);
        return Ok(history);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        var dashboardData = await _Productservice.GetProductstockData();
        return Ok(dashboardData);
    }

    // Get the current status of all SIM card requests
    [HttpGet("requests/status")]
    public async Task<IActionResult> GetAllRequestsStatus()
    {
        var requests = await _Productservice.GetAllRequestsWithStatus();
        var result = requests.Select(r => new
        {
            RequestId = r.Id,
            Name = r.product.Name,
            Status = r.Status,
            RequestedBy = r.RequestedBy,
            RequestDate = r.RequestDate
        });

        return Ok(result);
    }

    // Controller method for approving/rejecting a request
    [HttpPut("requests/{id}/process")]
    public async Task<IActionResult> ProcessProductRequest(int id, [FromBody] ProcessRequestModel model)
    {
        var success = await _Productservice.ProcessProductRequest(id, model.Action, model.ApprovedBy, model.RejectedBy);
        if (!success)
        {
            return BadRequest("Not enough stock, invalid request, or request has already been processed.");
        }

        return Ok($"Request {model.Action.ToLower()} successfully.");
    }

    [HttpGet("remaining-stock")]
    public async Task<IActionResult> GetRemainingStock()
    {
        var remainingStock = await _Productservice.GetRemainingStockAsync();
        return Ok(new { RemainingStock = remainingStock });
    }

}

public class ProductRequestModel
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string RequestedBy { get; set; }
}

public class ProcessRequestModel
{
    public string Action { get; set; }  // "Approved" or "Rejected"
    public string ApprovedBy { get; set; }
    public string RejectedBy { get; set; }
}


public class ApprovalModel
{
    public string ApprovedBy { get; set; }
    public string RejectedBy { get; set; }
}



