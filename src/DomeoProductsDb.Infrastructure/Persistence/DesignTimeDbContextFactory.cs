using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomeoProductsDb.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=domeo_products;Username=postgres;Password=postgres",
                npg => npg.MigrationsHistoryTable("__ef_migrations", "public"))
            .Options;
        return new ProductsDbContext(options);
    }
}
