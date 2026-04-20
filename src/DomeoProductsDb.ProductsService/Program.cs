using System.Text.Json.Serialization;
using DomeoProductsDb.Infrastructure.Persistence;
using DomeoProductsDb.Application.Abstractions;
using DomeoProductsDb.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DomeoProductsDb.ProductsService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured");

        builder.Services.AddDbContext<ProductsDbContext>(opts =>
            opts.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__ef_migrations", "public")));

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
        builder.Services.AddScoped<IBrandRepository, BrandRepository>();
        builder.Services.AddScoped<IAttributeRepository, AttributeRepository>();
        builder.Services.AddScoped<DatabaseSeeder>();

        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<Application.Categories.Queries.GetCategoriesQuery>());

        builder.Services
            .AddControllers()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync(CancellationToken.None);
        }

        app.MapOpenApi();
        app.UseStaticFiles();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapControllers();

        await app.RunAsync();
    }
}
