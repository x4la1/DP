using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace EventsLogger;

class Program
{
    private const string EventsExchangeName = "valuator.events";
    private const string RankEventName = "event.rank";
    private const string SimilarityEventName = "event.similarity";

    private static string queueName = "";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("EventsLogger started");

        ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclareTopologyAsync(channel);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (_, eventArgs) => await ConsumeAsync(channel, eventArgs);

        await channel.BasicConsumeAsync(
            queue: queueName, 
            autoAck: false, 
            consumer: consumer
        );

        Console.ReadLine();
    }

    private static async Task ConsumeAsync(IChannel channel, BasicDeliverEventArgs eventArgs)
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        string routingKey = eventArgs.RoutingKey;

        using JsonDocument doc = JsonDocument.Parse(message);
        JsonElement root = doc.RootElement;
        string id = root.GetProperty("Id").GetString() ?? "Unknown";

        if (routingKey == RankEventName)
        {
            double rank = root.GetProperty("Rank").GetDouble();
            Console.WriteLine($"RankCalculated ID: {id}, Rank: {rank}");
        }
        else if (routingKey == SimilarityEventName)
        {
            double similarity = root.GetProperty("Similarity").GetDouble();
            Console.WriteLine($"SimilarityCalculated ID: {id}, Similarity: {similarity}");
        }

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
    }

    private static async Task DeclareTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: EventsExchangeName,
            type: ExchangeType.Topic
            );

        QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync(
            queue: "",
            durable: true,
            exclusive: true,
            autoDelete: true
        );

        queueName = queueDeclareResult.QueueName;

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: EventsExchangeName,
            routingKey: "event.#"
        );

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
    }
}