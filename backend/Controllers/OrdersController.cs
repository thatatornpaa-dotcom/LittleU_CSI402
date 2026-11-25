using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    public record OrderItemInput(string ProductId, int Quantity);
    public record CreateOrderRequest(
        string? UserId,
        string RecipientName,
        string RecipientPhone,
        string ShippingAddress,
        decimal ShippingFee,
        List<OrderItemInput> Items
    );
    public record UpdateOrderStatusRequest(string Status);

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.Status,
                o.TotalPrice,
                o.ShippingFee,
                o.RecipientName,
                o.RecipientPhone,
                o.ShippingAddress,
                o.CreatedAt,
                itemCount = o.Items.Sum(i => i.Quantity)
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
    {
        if (req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "ไม่มีสินค้าในคำสั่งซื้อ" });

        if (string.IsNullOrWhiteSpace(req.RecipientName) || string.IsNullOrWhiteSpace(req.ShippingAddress))
            return BadRequest(new { message = "กรุณากรอกชื่อผู้รับและที่อยู่จัดส่ง" });

        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();

                var orderId = Guid.NewGuid().ToString();
                decimal itemsTotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in req.Items)
                {
                    var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                    if (product == null)
                        return (StatusCode: 404, Body: (object)new { message = $"ไม่พบสินค้า ID: {item.ProductId}" });

                    var qty = item.Quantity <= 0 ? 1 : item.Quantity;

                    if (product.Type == "READY_TO_SHIP" && product.Stock < qty)
                        return (StatusCode: 400, Body: (object)new { message = $"สินค้า {product.Name} คงเหลือไม่เพียงพอ" });

                    if (product.Type == "READY_TO_SHIP")
                    {
                        product.Stock -= qty;
                        product.UpdatedAt = DateTime.UtcNow;
                    }

                    var linePrice = product.Price * qty;
                    itemsTotal += linePrice;

                    orderItems.Add(new OrderItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderId = orderId,
                        ProductId = product.Id,
                        Quantity = qty,
                        Price = product.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                var shippingFee = req.ShippingFee < 0 ? 0 : req.ShippingFee;
                var resolvedUserId = string.IsNullOrWhiteSpace(req.UserId) ? "guest" : req.UserId.Trim();
                var userExists = await _db.Users.AnyAsync(u => u.Id == resolvedUserId);
                if (!userExists)
                {
                    var fallbackUser = await _db.Users.Select(u => u.Id).FirstOrDefaultAsync();
                    resolvedUserId = fallbackUser ?? resolvedUserId;
                }

                var order = new Order
                {
                    Id = orderId,
                    UserId = resolvedUserId,
                    Status = "PENDING",
                    ShippingFee = shippingFee,
                    TotalPrice = itemsTotal + shippingFee,
                    RecipientName = req.RecipientName.Trim(),
                    RecipientPhone = req.RecipientPhone?.Trim() ?? "",
                    ShippingAddress = req.ShippingAddress.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = orderItems
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return (StatusCode: 200, Body: (object)new
                {
                    message = "สร้างคำสั่งซื้อสำเร็จ",
                    orderId = order.Id,
                    totalPrice = order.TotalPrice,
                    status = order.Status
                });
            });

            return StatusCode(result.StatusCode, result.Body);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "เกิดข้อผิดพลาดในการสร้างคำสั่งซื้อ", detail = ex.InnerException?.Message ?? ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound(new { message = "ไม่พบคำสั่งซื้อนี้" });

        order.Status = req.Status.Trim().ToUpper();
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "อัปเดตสถานะสำเร็จ", order.Id, order.Status });
    }
}
