using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.AppDbContext;
using webapi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace webapi.Service;

public class Productservice
{
    
    private readonly ProductManagementContext _context;

    public Productservice(ProductManagementContext context)
    {
        _context = context;
    }


     // Create a new SIM card request with quantity
    public async Task<ProductRequest> CreateProductRequest(string requestedByUserId, int ProductId, int quantity)
    {
        var Product = await _context.Products.FindAsync(ProductId);
        if (Product == null || Product.Stock < quantity)
        {
            throw new Exception("Not enough stock available.");
        }

        var ProductRequest = new ProductRequest
        {
            
            RequestedByUserId = requestedByUserId, // Associate the request with the user ID
            ProductId = ProductId,
            Quantity = quantity,
            Status = "Initiated",
            RequestDate = DateTime.UtcNow
        };

        _context.ProductRequests.Add(ProductRequest);
        await _context.SaveChangesAsync();
        return ProductRequest;
    }

    // Get the remaining stock of SIM cards
    public async Task<int> GetRemainingStock()
    {
        return await _context.Products.SumAsync(s => s.Stock);
    }
    public async Task<bool> ProcessProductRequest(int requestId, string action, string approvedByUserId, string rejectedByUserId)
    {
        var ProductRequest = await _context.ProductRequests
            .Include(r => r.product) // Include related Product
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (ProductRequest == null || ProductRequest.Status != "Initiated")
        {
            return false;
        }

        // If the action is 'Approved', check stock availability
        if (action == "Approved")
        {
            // Check if enough stock is available
            if (ProductRequest.product.Stock < ProductRequest.Quantity)
            {
                return false; // Not enough stock to fulfill the request
            }

            // Deduct the requested quantity from the Product stock
            ProductRequest.product.Stock -= ProductRequest.Quantity;

            // Update the request status to 'Closed' after approval
            ProductRequest.Status = "Closed";
        }
        else
        {
            // If the request is rejected, set the status to 'Rejected'
            ProductRequest.Status = "Rejected";
        }

        // Log the approval/rejection history
        var approvalHistory = new ApprovalHistory
        {
            ProductRequestId = ProductRequest.Id,
            ApprovedBy = approvedByUserId,
            RejectedBy = rejectedByUserId,
            Action = action,
            ActionDate = DateTime.UtcNow
        };

        _context.ApprovalHistories.Add(approvalHistory);

        // Save all changes (sim card stock, request status, and approval history)
        await _context.SaveChangesAsync();

        return true;
    }

    // Get Approval history
    public async Task<List<ApprovalHistory>> GetApprovalHistory(int ProductRequestId)
    {
        return await _context.ApprovalHistories
            .Where(h => h.ProductRequestId == ProductRequestId)
            .ToListAsync();
    }

    // Get dashboard data
    public async Task<Dictionary<string, int>> GetProductstockData()
    {
        var totalStock = await _context.Products.CountAsync();
        var closedRequests = await _context.ProductRequests
            .CountAsync(r => r.Status == "Closed");

        return new Dictionary<string, int>
        {
            { "TotalProducts", totalStock },
            { "ClosedRequests", closedRequests }
        };
    }
    // Add new SIM card to stock
    public async Task<Product> AddProductAsync(Product newProduct)
    {
        // check if ICCID is null or stock is less than 1
        if (string.IsNullOrEmpty(newProduct.Name) || newProduct.Stock < 1)
        {
            throw new ArgumentException("Invalid ICCID or stock value");
        }
        // Add the new SIM card to the database
        await _context.Products.AddAsync(newProduct);
        await _context.SaveChangesAsync(); // Save changes to persist the new SIM card

        return newProduct; // Return the added SIM card
    }



    // Get SIM card by ID
    public async Task<Product> GetProductById(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(sc => sc.Id == id);
    }

    // Get all SIM cards
    public async Task<List<Product>> GetAllProducts()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> UpdateProductAsync(int id, Product updatedProduct)
    {
        // Check if the ID matches
        if (id != updatedProduct.Id)
        {
            throw new ArgumentException("ID mismatch");
        }

        // Fetch the existing SIM card from the database
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        // Update the properties
        existingProduct.Name = updatedProduct.Name;
        existingProduct.Stock = updatedProduct.Stock; // Ensure stock is set correctly
        existingProduct.Status = updatedProduct.Status;
        existingProduct.CreatedDate = updatedProduct.CreatedDate; // Assuming you want to keep the created date

        // Save changes to the database
        await _context.SaveChangesAsync();

        return existingProduct; // Return the updated SIM card
    }
    // Delete a SIM card
    public async Task<bool> DeleteProduct(int id)
    {
        var Product = await _context.Products.FindAsync(id);
        if (Product == null) return false;

        _context.Products.Remove(Product);
        await _context.SaveChangesAsync();
        return true;
    }

    // Get the current status of all SIM card requests
    public async Task<List<ProductRequest>> GetAllRequestsWithStatus(string userId = null)
    {
        IQueryable<ProductRequest> query = _context.ProductRequests
            .Include(r => r.product);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.RequestedByUserId == userId);
        }

        return await query.ToListAsync();
    }


    public async Task<int> GetRemainingStockAsync()
    {
        // Sum up the Stock field for all SIM cards to get the total available stock
        return await _context.Products.SumAsync(s => s.Stock);
    }


}
