using StackExchange.Redis;

namespace Valuator;

public class RedisShardingService
{
    private readonly IConnectionMultiplexer mainDb;
    private readonly Dictionary<string, IConnectionMultiplexer> shards = new();

    public RedisShardingService(IConfiguration config)
    {
        mainDb = ConnectionMultiplexer.Connect(config["DB_MAIN"] ?? "");

        shards["RU"] = ConnectionMultiplexer.Connect(config["DB_RU"] ?? "");
        shards["EU"] = ConnectionMultiplexer.Connect(config["DB_EU"] ?? "");
        shards["ASIA"] = ConnectionMultiplexer.Connect(config["DB_ASIA"] ?? "");
    }

    public IDatabase GetMainDb()
    {
        return mainDb.GetDatabase();
    }

    public IDatabase GetShardDb(string shardKey)
    {
        if (shards.TryGetValue(shardKey, out IConnectionMultiplexer? shard))
        {
            return shard.GetDatabase();
        }

        throw new ArgumentException($"Shard {shardKey} not found.");
    }
}

