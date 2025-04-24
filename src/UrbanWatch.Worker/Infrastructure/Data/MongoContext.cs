using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UrbanWatch.Worker.Documents;

namespace UrbanWatch.Worker.Infrastructure.Data;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoSettings> mongoSettings)
    {
        var settings = mongoSettings.Value;

        var client = new MongoClient($"mongodb://{settings.Username}:{settings.Password}@{settings.Host}:{settings.Port}");
        _database = client.GetDatabase(settings.Database);
    }
    public IMongoCollection<VehicleSnapshot> VehicleHistory => _database.GetCollection<VehicleSnapshot>("vehicles_live");
    public IMongoCollection<Route> Routes => _database.GetCollection<Route>("routes");
    public IMongoCollection<Shape> Shapes => _database.GetCollection<Shape>("shapes");
    public IMongoCollection<Stop> Stops => _database.GetCollection<Stop>("stops");
    public IMongoCollection<StopTimes> StopTimes => _database.GetCollection<StopTimes>("stop_times");
    public IMongoCollection<Trip> Trips => _database.GetCollection<Trip>("trips");
}