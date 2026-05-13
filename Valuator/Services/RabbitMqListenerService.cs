using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Valuator.Hubs;

namespace Valuator.Services;

public class RabbitMqListenerService : BackgroundService
{
    private readonly IHubContext<ResultHub> hubContext;
    private const string EventsExchangeName = "valuator.events";
    private string queueName = "";

    public RabbitMqListenerService(IHubContext<ResultHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };

        await using IConnection connection = await factory.CreateConnectionAsync();
        await using IChannel channel = await connection.CreateChannelAsync();

        await DeclateTopologyAsync(channel);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += async (_, eventArgs) => { await SendMessageAsync(eventArgs, channel); };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        await Task.Delay(-1);
    }

    private async Task SendMessageAsync(BasicDeliverEventArgs eventArgs, IChannel channel)
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

        RankCalculatedEvent? eventData = JsonSerializer.Deserialize<RankCalculatedEvent>(message);
        if (eventData != null)
        {
            await hubContext.Clients.Group(eventData.Id).SendAsync("ReceiveRank", eventData.Rank);
        }

        await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
    }

    private async Task DeclateTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(EventsExchangeName, ExchangeType.Topic);

        QueueDeclareOk queueResult = await channel.QueueDeclareAsync(
            queue: "",
            durable: true,
            exclusive: true,
            autoDelete: true);

        queueName = queueResult.QueueName;

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: EventsExchangeName,
            routingKey: "event.rank");
    }

}

