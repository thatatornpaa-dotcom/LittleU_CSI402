using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("User")]
public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = "BUYER";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Address> Addresses { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}

[Table("Address")]
public class Address
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Label { get; set; } = "ที่บ้าน";
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? Amphoe { get; set; }
    public string? Tambon { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Employee")]
public class Employee
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "staff";
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Product")]
public class Product
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? OriginalPrice { get; set; }

    public string Type { get; set; } = "READY_TO_SHIP";
    public int LeadTimeDays { get; set; } = 0;
    public string Category { get; set; } = "BRACELET";
    public int Stock { get; set; } = 0;

    [Column(TypeName = "decimal(4,1)")]
    public decimal? WristSizeMin { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? WristSizeMax { get; set; }

    [Column(TypeName = "decimal(2,1)")]
    public decimal Rating { get; set; } = 0.0m;

    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string ModerationStatus { get; set; } = "VISIBLE";
    public string? ModerationReason { get; set; }
    public string? ModeratedById { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ProductImage> Images { get; set; } = new();
}

[Table("ProductImage")]
public class ProductImage
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProductId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Position { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Category")]
public class Category
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Accessory")]
public class Accessory
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Order")]
public class Order
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ShippingFee { get; set; } = 0.00m;

    public string ShippingAddress { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}

[Table("OrderItem")]
public class OrderItem
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public string? SubOrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
