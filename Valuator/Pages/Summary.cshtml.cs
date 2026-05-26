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
    private readonly RedisShardingService _redisService;

    public SummaryModel(ILogger<SummaryModel> logger, RedisShardingService redisService)
    {
        _logger = logger;
        _redisService = redisService;
    }

    public string Rank { get; set; } = "Оценка содержания не завершена";
    public double Similarity { get; set; }

    public async Task OnGet(string id)
    {
        IDatabase mainDb = _redisService.GetMainDb();

        RedisValue regionVal = await mainDb.StringGetAsync("SHARD-" + id);
        string region = regionVal.ToString();

        _logger.LogInformation($"LOOKUP: {id}, {region}");

        if (!string.IsNullOrEmpty(region))
        {
            IDatabase shardDb = _redisService.GetShardDb(region);

            RedisValue similarity = await shardDb.StringGetAsync("SIMILARITY-" + id);
            Similarity = (double)similarity;

            RedisValue rank = await shardDb.StringGetAsync("RANK-" + id);
            if (rank.HasValue)
            {
                Rank = Math.Round((double)rank, 2).ToString();
            }
        }
    }
}
