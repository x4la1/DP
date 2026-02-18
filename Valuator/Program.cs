using StackExchange.Redis;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string redisConnection = builder.Configuration["REDIS_CONNECTION"] ?? "";

        builder.Services.AddSingleton<IConnectionMultiplexer>(
                sp => ConnectionMultiplexer.Connect(redisConnection)
            );

        builder.Services.AddScoped(
                sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
            );

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
