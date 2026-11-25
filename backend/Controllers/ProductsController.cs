using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    public record CreateProductRequest(
        string Name,
        string? Description,
        decimal Price,
        string? Type,
        string? Category,
        int Stock,
        string? ImageUrl
    );

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        var query = _db.Products
            .Include(p => p.Images.OrderBy(i => i.Position))
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.Trim().ToLower()));

        var list = await query
            .OrderBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.OriginalPrice,
                p.Type,
                p.LeadTimeDays,
                p.Category,
                p.Stock,
                p.WristSizeMin,
                p.WristSizeMax,
                p.Rating,
                p.ReviewCount,
                p.IsActive,
                p.ModerationStatus,
                imageUrl = p.Images.OrderBy(i => i.Position).Select(i => i.Url).FirstOrDefault(),
                images = p.Images.OrderBy(i => i.Position).Select(i => new { i.Id, i.Url, i.Position })
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || req.Price <= 0)
            return BadRequest(new { message = "กรุณาระบุชื่อสินค้าและราคาที่ถูกต้อง" });

        try
        {
            var productId = Guid.NewGuid().ToString();
            var product = new Product
            {
                Id = productId,
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                Price = req.Price,
                Type = string.IsNullOrWhiteSpace(req.Type) ? "READY_TO_SHIP" : req.Type.Trim(),
                Category = string.IsNullOrWhiteSpace(req.Category) ? "BRACELET" : req.Category.Trim(),
                Stock = req.Stock < 0 ? 0 : req.Stock,
                IsActive = true,
                IsDeleted = false,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(req.ImageUrl))
            {
                _db.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    Url = req.ImageUrl.Trim(),
                    Position = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = "เพิ่มสินค้าเรียบร้อยแล้ว", product.Id, product.Name, product.Price });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "เพิ่มสินค้าไม่สำเร็จ", detail = ex.InnerException?.Message ?? ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return NotFound(new { message = "ไม่พบสินค้านี้" });

        product.IsDeleted = true;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "ลบสินค้าเรียบร้อยแล้ว" });
    }
}
