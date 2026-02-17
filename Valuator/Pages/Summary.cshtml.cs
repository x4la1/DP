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

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        RedisValue rank = _redis.StringGet("RANK-" + id);
        RedisValue similarity = _redis.StringGet("SIMILARITY-" + id);

        Rank = rank.HasValue ? (double)rank : 0.0;
        Similarity = similarity.HasValue ? (double)similarity : 0.0;
    }
}
