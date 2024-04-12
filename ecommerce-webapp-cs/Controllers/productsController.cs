using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_webapp_cs.Models.ProductModels;
using System.Data.SqlClient;
using OfficeOpenXml;


namespace ecommerce_webapp_cs.Controllers;


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
        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        return Ok(products);
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        try
        {
            var product = await _context.Products.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProId == id);

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
            // map ProductModel to product entity
            var product = new Product
            {
                ProName = productDto.ProName,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                CategoryId = productDto.CategoryId,
                Image1 = productDto.Image1,
                Image2 = productDto.Image2,
                Image3 = productDto.Image3
            };

            // check if the CategoryId is valid
            var category = await _context.ProductCategories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid CategoryId");
            }

            // Generate ProId
            product.ProId = $"{product.CategoryId}{GenerateRandomString(3)}";

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
            if (product == null) return NotFound();

            product.ProName = productDto.ProName;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;
            product.Image1 = productDto.Image1;
            product.Image2 = productDto.Image2;
            product.Image3 = productDto.Image3;

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
        // Ensure length is not greater than the size of the ProID column in the database.
        const int maxProIdLength = 6;
        length = Math.Min(length, maxProIdLength);

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // GET: api/v1/Products/Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.ProductCategories.ToListAsync();
        return Ok(categories);
    }

    // GET: api/v1/Products/Categories/5
    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var category = await _context.ProductCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);

        }
        catch (DbUpdateException ex)
        {
            return HandleDbUpdateException(ex);
        }
    }

    // POST: api/v1/Products/Categories
    [HttpPost("categories")]
    public async Task<IActionResult> PostCategory([FromBody] CategoryModel categoryDto)
    {
        try
        {
            var category = new ProductCategory
            {
                CategoryName = categoryDto.CategoryName
            };

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }
        catch (DbUpdateException ex)
        {
            return HandleDbUpdateException(ex);
        }
    }

    // PUT: api/v1/Products/Categories/5
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryModel categoryDto)
    {
        try
        {
            var category = await _context.ProductCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = categoryDto.CategoryName;

            _context.ProductCategories.Update(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProductCategories.Any(e => e.CategoryId == id))
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

    // DELETE: api/v1/Products/Categories/5
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        try
        {
            _context.ProductCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // check if the exception is due to a foreign key constraint
            if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2627))
            {
                return BadRequest("Cannot delete this category because it is associated with one or more discounts.");
            }
            throw; // if it's not the expected exception type or code, rethrow it
        }

        return Ok("Category deleted successfully.");
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
                        int categoryId;
                        if (!int.TryParse(worksheet.Cells[row, 5].Value?.ToString().Trim(), out categoryId))
                        {
                            continue;
                        }

                        var category = await _context.ProductCategories.FindAsync(categoryId);
                        if (category == null)
                        {
                            continue; // skip this product
                        }

                        var product = new Product
                        {
                            ProId = GenerateRandomString(6), // assuming a method exists to generate a unique ProId
                            ProName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                            Description = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                            Price = decimal.Parse(worksheet.Cells[row, 3].Value?.ToString().Trim()),
                            StockQuantity = int.Parse(worksheet.Cells[row, 4].Value?.ToString().Trim()),
                            CategoryId = categoryId
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
        // Set LicenseContext for EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var products = await _context.Products.Include(p => p.Category).ToListAsync();

        var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            var workSheet = package.Workbook.Worksheets.Add("Products");
            workSheet.Cells.LoadFromCollection(products.Select(p => new
            {
                p.ProName,
                p.Description,
                p.Price,
                p.StockQuantity,
                CategoryName = p.Category?.CategoryName // assuming there is a navigation property to Category
            }), true, OfficeOpenXml.Table.TableStyles.Light1);

            // auto-fit columns for all cells
            workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();

            package.Save();
        }

        stream.Position = 0;
        string excelName = $"Products-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

        // return the Excel file as a download
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }


}