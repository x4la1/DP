using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IDatabase _redis;

    public RegisterModel(IDatabase redis) { _redis = redis; }

    public async Task<IActionResult> OnPostAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Page();
        }

        string userKey = $"USER-{login}";

        if (await _redis.KeyExistsAsync(userKey))
        {
            return Page();
        }

        var hasher = new PasswordHasher<string>();
        var hashedPassword = hasher.HashPassword(login, password);

        await _redis.StringSetAsync(userKey, hashedPassword);
        return RedirectToPage("/Login");
    }
}

