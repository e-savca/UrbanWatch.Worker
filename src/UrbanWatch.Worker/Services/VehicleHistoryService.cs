// using MongoDB.Driver;
// using UrbanWatch.Worker.DTOs;
// using UrbanWatch.Worker.Infrastructure.Mongo;
//
// namespace UrbanWatch.Worker.Services;
//
// public class VehicleHistoryService
// {
//     private readonly IMongoCollection<VehicleSnapshot> _collection;
//
//     public VehicleHistoryService(MongoContext mongoContext)
//     {
//         _collection = mongoContext.VehicleHistory;
//     }
//     public async Task SaveBatchAsync(List<Vehicle> vehicles, CancellationToken ct)
//     {
//         var snapshot = new VehicleSnapshot
//         {
//             Timestamp = DateTime.UtcNow,
//             Vehicles = vehicles
//         };
//
//         await _collection.InsertOneAsync(snapshot, cancellationToken: ct);
//     }
//
//     public async Task ClearAsync(CancellationToken ct)
//     {
//         await _collection.DeleteManyAsync(FilterDefinition<VehicleSnapshot>.Empty, cancellationToken: ct);
//     }
//
//     public async Task ClearAsync(TimeSpan timeSpan, CancellationToken ct)
//     {
//         var threshold = DateTime.UtcNow - timeSpan;
//         var filter = Builders<VehicleSnapshot>.Filter.Lt(x => x.Timestamp, threshold);
//         await _collection.DeleteManyAsync(filter, cancellationToken: ct);
//     }
//
//
// }
