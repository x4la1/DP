using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;

    private const string ExchangeName = "valuator.processing.rank";
    private const string QueueName = "valuator.processing.rank";
    private const string EventsExchangeName = "valuator.events";
    private const string SimilarityEventName = "event.similarity";

    public IndexModel(ILogger<IndexModel> logger, IDatabase redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        if (!string.IsNullOrEmpty(text))
        {
            string similarityKey = "SIMILARITY-" + id;
            double similarity = CalculateSimilarity(text);
            await _redis.StringSetAsync(similarityKey, similarity);

            var similarityEventPayload = new { Id = id, Similarity = similarity };
            await PublishSimilarityCalculatedEventAsync(similarityEventPayload);

            string textKey = "TEXT-" + id;
            await _redis.StringSetAsync(textKey, text);

            await PublishRankTaskAsync(id);
        }

        return Redirect($"summary?id={id}");
    }

    private async Task PublishSimilarityCalculatedEventAsync(object payload)
    {
        ConnectionFactory connectionFactory = new ConnectionFactory { HostName = "localhost" };
        await using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: EventsExchangeName,
            type: ExchangeType.Topic
        );

        string jsonMessage = JsonSerializer.Serialize(payload);
        byte[] messageData = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync(
            exchange: EventsExchangeName,
            routingKey: SimilarityEventName,
            mandatory: false,
            body: messageData
        );
    }

    private async Task PublishRankTaskAsync(string id)
    {
        ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel, CancellationToken.None);

        byte[] messageData = Encoding.UTF8.GetBytes(id);

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "",
            mandatory: false,
            body: messageData
        );
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

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            cancellationToken: ct
        );
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: "",
            cancellationToken: ct
        );
    }

}
