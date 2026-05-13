using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double? Rank { get; set; }
    public double Similarity { get; set; }

    public string Id { get; set; } = "";

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        Id = id;

        RedisValue similarity = _redis.StringGet("SIMILARITY-" + id);
        Similarity = (double)similarity;

        RedisValue rank = _redis.StringGet("RANK-" + id);
        if (rank.HasValue)
        {
            Rank = Math.Round((double)rank, 2);
        }
    }
}
