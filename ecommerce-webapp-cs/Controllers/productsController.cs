using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_webapp_cs.Models.ProductModels;
using System.Data.SqlClient;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ecommerce_webapp_cs.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class productsController : ControllerBase
    {
        private readonly ArtsContext _context;

        public productsController(ArtsContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProId == id);

                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (DbUpdateException ex)
            {
                return HandleDbUpdateException(ex);
            }
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] ProductModel productDto)
        {
            try
            {
                var product = new Product
                {
                    ProName = productDto.ProName,
                    Description = productDto.Description,
                    ProImg1 = productDto.ProImg1,
                    ProImg2 = productDto.ProImg2,
                    ProImg3 = productDto.ProImg3,
                    Price = productDto.Price,
                    StockQuantity = productDto.StockQuantity,
                };

                // Generate ProId
                product.ProId = GenerateRandomString(6);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProduct", new { id = product.ProId }, product);
            }
            catch (DbUpdateException ex)
            {
                return HandleDbUpdateException(ex);
            }
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, [FromBody] ProductEditModel productDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                product.ProName = productDto.ProName;
                product.Description = productDto.Description;
                product.Price = productDto.Price;
                product.StockQuantity = productDto.StockQuantity;
                _context.Products.Update(product);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return HandleDbUpdateException(ex);
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (DbUpdateException ex)
            {
                return HandleDbUpdateException(ex);
            }
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProId == id);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private IActionResult HandleDbUpdateException(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2627))
            {
                return BadRequest("Operation failed due to a database constraint. Details: " + sqlEx.Message);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Details: " + ex.Message);
            }
        }
    }
}

