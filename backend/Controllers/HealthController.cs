using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync();
            var productCount = await _db.Products.CountAsync(p => !p.IsDeleted);
            return Ok(new
            {
                status = "ok",
                store = "Little U Accessories Shop",
                databaseConnected = canConnect,
                activeProducts = productCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                databaseConnected = false,
                message = ex.Message
            });
        }
    }
}
