using Microsoft.AspNetCore.Mvc;
using webapi.Models;
using webapi.Service;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace webapi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductstockController : ControllerBase
    {
        private readonly Productservice _Productservice;

        public ProductstockController(Productservice Productservice)
        {
            _Productservice = Productservice;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product newProduct)
        {
            try
            {
                var addedCard = await _Productservice.AddProductAsync(newProduct);
                return CreatedAtAction(nameof(AddProduct), new { id = addedCard.Id }, addedCard); // Return 201 Created
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Return a bad request if the input is invalid
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Return a 500 error for unexpected issues
            }
        }

        // Get all SIM cards
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var Products = await _Productservice.GetAllProducts();
            return Ok(Products);
        }

        // Get a SIM card by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var Product = await _Productservice.GetProductById(id);
            if (Product == null)
                return NotFound("SIM card not found.");
            return Ok(Product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            try
            {
                var updatedCard = await _Productservice.UpdateProductAsync(id, updatedProduct);
                return Ok(updatedCard); // Return the updated SIM card
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // Return a bad request if there's an ID mismatch
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // Return 404 if the SIM card was not found
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Return a 500 error for unexpected issues
            }
        }

        // Delete a SIM card
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _Productservice.DeleteProduct(id);
            if (!result)
                return NotFound("SIM card not found.");

            return Ok("SIM card deleted successfully.");
        }
    }

}
