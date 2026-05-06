using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    private const string QueueName = "valuator.processing.rank";
    private const string EventsExchangeName = "valuator.events";
    private const string RankEventName = "event.rank";
    private static IDatabase _redis;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Consumer started");

        ConnectionMultiplexer redisConnection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        _redis = redisConnection.GetDatabase();

        ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel);

        string consumerTag = await RunConsumer(channel);

        Console.WriteLine("Press Enter to exit");
        Console.ReadLine();

        await channel.BasicCancelAsync(consumerTag);
        Console.WriteLine("Done");
    }

    private static async Task<string> RunConsumer(IChannel channel)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (_, eventArgs) => await ConsumeAsync(channel, eventArgs);

        return await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer
        );
    }

    private static async Task ConsumeAsync(IChannel channel, BasicDeliverEventArgs eventArgs)
    {
        string id = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        RedisValue textValue = await _redis.StringGetAsync("TEXT-" + id);
        string text = textValue.ToString();

        double rank = CalculateRank(text);
        Console.WriteLine($"Calculated Rank: {rank}");
        await _redis.StringSetAsync("RANK-" + id, rank);

        var rankEventPayload = new { Id = id, Rank = rank };
        await PublishRankCalculatedEventAsync(channel, rankEventPayload);

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
        Console.WriteLine("Task completed");
    }

    private static async Task PublishRankCalculatedEventAsync(IChannel channel, object payload)
    {
        await channel.ExchangeDeclareAsync(exchange: EventsExchangeName, type: ExchangeType.Topic);

        string jsonMessage = JsonSerializer.Serialize(payload);
        byte[] body = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync(
            exchange: EventsExchangeName,
            routingKey: RankEventName,
            mandatory: false,
            body: body
        );
    }

    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    private static double CalculateRank(string text)
    {
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