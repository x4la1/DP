using StackExchange.Redis;

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

        var app = builder.Build();

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

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0.0;
        }

        int nonAlphabetCharsCount = 0;

        foreach (char c in text)
        {
            if (!char.IsLetter(c))
            {
                nonAlphabetCharsCount++;
            }
        }

        return (double)nonAlphabetCharsCount / text.Length;
    }
}
