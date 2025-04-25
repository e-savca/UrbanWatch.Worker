using StackExchange.Redis;

namespace UrbanWatch.Worker.Infrastructure.Redis;

public class RedisContext : IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private bool _disposed;

    public RedisContext(IConfiguration configuration)
    {
        var connectionString = configuration["REDIS_CONNECTION_STRING"];
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ApplicationException("REDIS_CONNECTION_STRING is empty");
        _redis = ConnectionMultiplexer.Connect(connectionString);
    }
    public IDatabase Database => _redis.GetDatabase(0);
    public ISubscriber Subscriber => _redis.GetSubscriber();
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _redis.Dispose();
            _disposed = true;
        }
    }
}