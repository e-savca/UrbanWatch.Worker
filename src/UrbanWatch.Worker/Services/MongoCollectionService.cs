using System;
using MongoDB.Driver;
using UrbanWatch.Worker.Infrastructure.Mongo;
using UrbanWatch.Worker.Infrastructure.Mongo.Documents;

namespace UrbanWatch.Worker.Services;

public class MongoCollectionService
{
    private readonly IMongoCollection<Route> _routes;
    private readonly IMongoCollection<Shape> _shapes;
    private readonly IMongoCollection<Stop> _stops;
    private readonly IMongoCollection<StopTimes> _stopTimes;
    private readonly IMongoCollection<Trip> _trips;

    public MongoCollectionService(MongoContext mongoContext)
    {
        _routes = mongoContext.Routes;
        _shapes = mongoContext.Shapes;
        _stops = mongoContext.Stops;
        _stopTimes = mongoContext.StopTimes;
        _trips = mongoContext.Trips;
    }

    private async Task UpdateAsync<T>(IMongoCollection<T> collection, List<T> items, CancellationToken ct)
    {
        await collection.DeleteManyAsync(FilterDefinition<T>.Empty, cancellationToken: ct);
        await collection.InsertManyAsync(items, cancellationToken: ct);
    }

    public async Task UpdateRoutesAsync(List<Route> routes, CancellationToken ct)
    {
        await UpdateAsync(_routes, routes, ct);
    }
    public async Task UpdateShapesAsync(List<Shape> shape, CancellationToken ct)
    {
        await UpdateAsync(_shapes, shape, ct);
    }

    public async Task UpdateStopsAsync(List<Stop> stops, CancellationToken ct)
    {
        await UpdateAsync(_stops, stops, ct);
    }
    public async Task UpdateStopTimesAsync(List<StopTimes> stopTimes, CancellationToken ct)
    {
        await UpdateAsync(_stopTimes, stopTimes, ct);
    }
    public async Task UpdateTripsAsync(List<Trip> trips, CancellationToken ct)
    {
        await UpdateAsync(_trips, trips, ct);
    }
}
