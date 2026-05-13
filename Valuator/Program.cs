using StackExchange.Redis;
using Valuator.Hubs;
namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string redisConnection = builder.Configuration["REDIS_CONNECTION"] ?? "";

        builder.Services.AddSingleton<IConnectionMultiplexer>
            (
                sp => ConnectionMultiplexer.Connect(redisConnection)
            );

        builder.Services.AddScoped
            (
                sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
            );

        builder.Services.AddRazorPages();

        builder.Services.AddSignalR();
        builder.Services.AddHostedService<Services.RankEventListenerService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.MapHub<ResultHub>("/resultHub");

        app.Run();
    }
}
