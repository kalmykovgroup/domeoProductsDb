namespace DomeoProductsDb.ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", role = "api-gateway" }));
        app.MapReverseProxy();

        app.Run();
    }
}
