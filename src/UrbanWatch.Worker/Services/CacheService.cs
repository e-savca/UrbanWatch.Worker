// using Newtonsoft.Json;
// using Newtonsoft.Json.Serialization;
// using UrbanWatch.Worker.DTOs;
// using UrbanWatch.Worker.Infrastructure.Redis;
//
// namespace UrbanWatch.Worker.Services;
//
// public class CacheService(RedisContext redisContext)
// {
//     public async Task CacheVehiclesAsync(List<Vehicle> vehicles)
//     {
//         var settings = new JsonSerializerSettings
//         {
//             ContractResolver = new DefaultContractResolver
//             {
//                 NamingStrategy = new DefaultNamingStrategy(),
//                 IgnoreSerializableAttribute = true
//             }
//         };
//
//         var serializedVehicles = JsonConvert.SerializeObject(vehicles, settings);
//
//         await redisContext.Database.StringSetAsync("vehicles_live", serializedVehicles);
//
//         await redisContext.Subscriber.PublishAsync("vehicles_live:updated", "vehicles_live updated");
//     }
//
//     public async Task<List<Vehicle>?> GetCachedVehiclesAsync()
//     {
//         var value = await redisContext.Database.StringGetAsync("vehicles_live");
//
//         if (!value.HasValue)
//             return null;
//
//         var settings = new JsonSerializerSettings
//         {
//             ContractResolver = new DefaultContractResolver
//             {
//                 NamingStrategy = new DefaultNamingStrategy(),
//                 IgnoreSerializableAttribute = true
//             }
//         };
//
//         return JsonConvert.DeserializeObject<List<Vehicle>>(value, settings);
//     }
//
// }