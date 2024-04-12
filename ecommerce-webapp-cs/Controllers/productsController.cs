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
using Microsoft.AspNetCore.Authorization;


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
            var products = await _context.Products
                                         .OrderByDescending(p => p.CreationDate) 
                                         .ToListAsync();
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
        public async Task<IActionResult> PostProduct([FromForm] ProductModel productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (productDto.Price < 1 || productDto.StockQuantity < 1)
            {
                return BadRequest("Price and Stock Quantity must be greater than 0.");
            }

            try
            {
                var product = new Product
                {
                    ProName = productDto.ProName,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    StockQuantity = productDto.StockQuantity,
                    CreationDate = DateTime.UtcNow,
                    ProImg1 = await SaveImage(productDto.ProImg1),
                    ProImg2 = await SaveImage(productDto.ProImg2),
                    ProImg3 = await SaveImage(productDto.ProImg3),
                };

                // Generate ProId if necessary
                product.ProId = GenerateRandomString(7);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProId }, product);
            }
            catch (Exception ex)
            {
                // Log the exception (Consider using a logging framework/library)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // check if the file is an image
                if (!imageFile.ContentType.StartsWith("image/"))
                {
                    throw new ArgumentException("Only image files are allowed.");
                }

                // check if the image size is less than 5MB
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("Image size cannot exceed 5MB.");
                }

                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images");

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var extension = Path.GetExtension(imageFile.FileName);
                var newFileName = $"{Guid.NewGuid()}{extension}"; // generate a new file name to prevent overwriting
                var filePath = Path.Combine(imagesPath, newFileName);

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    return newFileName; // returning the new file name or a relative path as needed
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred while saving the image file.", ex);
                }
            }

            return null; // return null if no image was provided or if it didn't pass the checks
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (file == null || file.Length <= 0)
            {
                return BadRequest("Upload a valid Excel file.");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File format not supported.");
            }

            var productList = new List<Product>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var product = new Product
                            {
                                ProId = GenerateRandomString(6),
                                ProName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                                Description = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                                Price = decimal.Parse(worksheet.Cells[row, 3].Value?.ToString().Trim()),
                                StockQuantity = int.Parse(worksheet.Cells[row, 4].Value?.ToString().Trim()),
                                ProImg1 = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                                ProImg2 = worksheet.Cells[row, 6].Value?.ToString().Trim(), 
                                ProImg3 = worksheet.Cells[row, 7].Value?.ToString().Trim(), 
                            };

                            productList.Add(product);
                        }

                        if (productList.Any())
                        {
                            _context.Products.AddRange(productList);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return BadRequest("No valid products found in the uploaded file.");
                        }
                    }
                }

                return Ok($"{productList.Count} products imported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [HttpGet("export")]
        public async Task<IActionResult> ExportProductsToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var products = _context.Products.ToList();


            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Products");

                workSheet.Cells.LoadFromCollection(products.Select(p => new
                {
                    Name = p.ProName,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ProImg1 = p.ProImg1,
                    ProImg2 = p.ProImg2,
                    ProImg3 = p.ProImg3,

                }), true, OfficeOpenXml.Table.TableStyles.Light1);

                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

                package.Save();
            }

            stream.Position = 0;
            string excelName = $"Products-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

    }
}

