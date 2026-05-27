using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;

[Authorize]
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public string Rank { get; set; } = "Оценка содержания не завершена";
    public double Similarity { get; set; }

    public IActionResult OnGet(string id)
    {
        string currentUser = User.Identity?.Name ?? "";

        RedisValue author = _redis.StringGet("AUTHOR-" + id);

        if(author.HasValue && author.ToString() != currentUser)
        {
            return RedirectToPage("/Index");
        }

        _logger.LogDebug(id);

        RedisValue similarity = _redis.StringGet("SIMILARITY-" + id);
        Similarity = (double)similarity;

        RedisValue rank = _redis.StringGet("RANK-" + id);
        if (rank.HasValue)
        {
            Rank = Math.Round((double)rank, 2).ToString();
        }

        return Page();
    }
}
