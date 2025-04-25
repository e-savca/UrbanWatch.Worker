using UrbanWatch.Worker.Infrastructure.Redis;

namespace UrbanWatch.Worker.Services;

public class CacheChangeListener(RedisContext redisContext)
{
    public void StartListening()
    {
        redisContext.Subscriber.Subscribe("vehicles_live:updated", (channel, message) =>
        {
            Console.WriteLine($"Cache updated: {message}");
        });
    }
}
