using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;

    public IndexModel(ILogger<IndexModel> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text);
        _redis.StringSet(similarityKey, similarity);

        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey
        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        _redis.StringSet(rankKey, rank);

        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        string textKey = "TEXT-" + id;
        _redis.StringSet(textKey, text);

        return Redirect($"summary?id={id}");
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
            bool isLatin = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
            bool isRussian = (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';

            if (!isLatin && !isRussian)
            {
                nonAlphabetCharsCount++;
            }
        }

        return (double)nonAlphabetCharsCount / text.Length;
    }

    private double CalculateSimilarity(string text)
    {
        IServer server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
        RedisKey[] textKeys = server.Keys(pattern: "TEXT-*").ToArray();

        foreach (RedisKey key in textKeys)
        {
            RedisValue currentText = _redis.StringGet(key);
            if (currentText.HasValue && currentText.ToString() == text)
            {
                return 1.0;
            }
        }

        return 0.0;
    }
}
