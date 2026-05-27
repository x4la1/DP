using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using System.Security.Claims;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly IDatabase _redis;

    public LoginModel(IDatabase redis)
    {
        _redis = redis;
    }

    public async Task<IActionResult> OnPostAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return Page();

        string userKey = $"USER-{login}";

        RedisValue dbPasswordHash = await _redis.StringGetAsync(userKey);

        if (!dbPasswordHash.HasValue)
        {
            return Page();
        }

        var hasher = new PasswordHasher<string>();
        var verificationResult = hasher.VerifyHashedPassword(login, dbPasswordHash.ToString(), password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToPage("/Index");
    }
}

