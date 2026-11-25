using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Accessory> Accessories => Set<Accessory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ShopId).HasColumnName("shopId");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.OriginalPrice).HasColumnName("originalPrice");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.LeadTimeDays).HasColumnName("leadTimeDays");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.Ignore(e => e.WristSizeMin);
            entity.Ignore(e => e.WristSizeMax);
            entity.Ignore(e => e.Rating);
            entity.Ignore(e => e.ReviewCount);
            entity.Ignore(e => e.ModerationStatus);
            entity.Ignore(e => e.ModeratedAt);
            entity.Ignore(e => e.ModeratedById);
            entity.Ignore(e => e.ModerationReason);
            entity.Ignore(e => e.CreatedAt);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImage");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Url).HasColumnName("url");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalPrice).HasColumnName("totalPrice");
            entity.Property(e => e.ShippingFee).HasColumnName("shippingFee");
            entity.Property(e => e.ShippingAddress).HasColumnName("shippingAddress");
            entity.Property(e => e.RecipientName).HasColumnName("recipientName");
            entity.Property(e => e.RecipientPhone).HasColumnName("recipientPhone");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.SubOrderId).HasColumnName("subOrderId");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.FirstName).HasColumnName("firstName");
            entity.Property(e => e.LastName).HasColumnName("lastName");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });
    }
}
