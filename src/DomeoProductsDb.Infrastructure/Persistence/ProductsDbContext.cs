using DomeoProductsDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.Infrastructure.Persistence;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductAttribute> Attributes => Set<ProductAttribute>();
    public DbSet<EnumValue> EnumValues => Set<EnumValue>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<SupplierOffer> SupplierOffers => Set<SupplierOffer>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>(e =>
        {
            e.ToTable("categories", "reference");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.TitleRu).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Brand>(e =>
        {
            e.ToTable("brands", "reference");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.TitleRu).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.TitleRu).IsUnique();
        });

        b.Entity<Supplier>(e =>
        {
            e.ToTable("suppliers", "reference");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(255).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.Website).HasMaxLength(255);
            e.Property(x => x.Address).HasMaxLength(512);
            e.Property(x => x.Country).HasMaxLength(64);
            e.Property(x => x.Inn).HasMaxLength(32);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<ProductAttribute>(e =>
        {
            e.ToTable("attributes", "reference");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.TitleRu).HasMaxLength(255).IsRequired();
            e.Property(x => x.ValueType)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        b.Entity<EnumValue>(e =>
        {
            e.ToTable("enum_values", "reference");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.TitleRu).HasMaxLength(255).IsRequired();
            e.HasIndex(x => new { x.AttributeId, x.Code }).IsUnique();
            e.HasOne(x => x.Attribute).WithMany(x => x.EnumValues)
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Product>(e =>
        {
            e.ToTable("products", "staging");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.ExternalCode).HasMaxLength(100).IsRequired();
            e.Property(x => x.NameRu).HasMaxLength(500).IsRequired();
            e.Property(x => x.MainImageFilename).HasMaxLength(64);
            e.HasIndex(x => x.ExternalCode).IsUnique();
            e.HasIndex(x => x.CategoryId);
            e.HasOne(x => x.Category).WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductAttributeValue>(e =>
        {
            e.ToTable("product_attribute_values", "staging", t =>
            {
                t.HasCheckConstraint(
                    "ck_pav_type_exclusive",
                    "(value_type = 'Text'    AND value_text IS NOT NULL AND value_numeric IS NULL AND value_bool IS NULL AND enum_value_id IS NULL AND brand_id IS NULL)"
                    + " OR (value_type = 'Numeric' AND value_numeric IS NOT NULL AND value_text IS NULL AND value_bool IS NULL AND enum_value_id IS NULL AND brand_id IS NULL)"
                    + " OR (value_type = 'Bool'    AND value_bool IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND enum_value_id IS NULL AND brand_id IS NULL)"
                    + " OR (value_type = 'Enum'    AND enum_value_id IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND value_bool IS NULL AND brand_id IS NULL)"
                    + " OR (value_type = 'Brand'   AND brand_id IS NOT NULL AND value_text IS NULL AND value_numeric IS NULL AND value_bool IS NULL AND enum_value_id IS NULL)");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.AttributeId).HasColumnName("attribute_id");
            e.Property(x => x.ValueType)
                .HasColumnName("value_type")
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            e.Property(x => x.ValueText).HasColumnName("value_text");
            e.Property(x => x.ValueNumeric).HasColumnName("value_numeric").HasColumnType("numeric(18,4)");
            e.Property(x => x.ValueBool).HasColumnName("value_bool");
            e.Property(x => x.EnumValueId).HasColumnName("enum_value_id");
            e.Property(x => x.BrandId).HasColumnName("brand_id");
            e.HasIndex(x => new { x.ProductId, x.AttributeId });
            e.HasOne(x => x.Product).WithMany(x => x.Attributes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Attribute).WithMany()
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EnumValue).WithMany()
                .HasForeignKey(x => x.EnumValueId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand).WithMany()
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<SupplierOffer>(e =>
        {
            e.ToTable("supplier_offers", "staging");
            e.HasKey(x => x.Id);
            e.Property(x => x.PriceAmount).HasColumnType("numeric(18,2)");
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.HasIndex(x => x.ProductId);
            e.HasOne(x => x.Product).WithMany(x => x.Offers)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Supplier).WithMany(x => x.Offers)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
