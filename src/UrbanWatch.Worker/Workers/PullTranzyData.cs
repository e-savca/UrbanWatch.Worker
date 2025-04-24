using System;
using UrbanWatch.Worker.Clients;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;

namespace UrbanWatch.Worker.Workers;

public class PullTranzyData(
    MongoCollectionService mongoService,
    TranzyClient tranzyClient,
    TimeWindowHelper timeWindowHelper,
    ILogger<PullTranzyData> logger
) : BackgroundService
{
    private const string AgencyId = "4";
    private readonly TimeSpan[] _timesVar = new[]
    {
        new TimeSpan(4, 0, 0) ,
        new TimeSpan(8, 0, 0) ,
    };

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var tasks = new[]
            {
                UpdateRoutes(cancellationToken),
                UpdateShapes(cancellationToken),
                UpdateStops(cancellationToken),
                UpdateStopTimes(cancellationToken),
                UpdateTrips(cancellationToken)
            };

            await Task.WhenAll(tasks);
            var delay = timeWindowHelper.GetDelay(_timesVar);

            await Task.Delay(timeWindowHelper.GetDelay(_timesVar));
        }
    }

    private async Task UpdateTrips(CancellationToken cancellationToken)
    {
        var trips = await tranzyClient.GetTripsAsync(AgencyId);
        await mongoService.UpdateTripsAsync(trips, cancellationToken);
    }

    private async Task UpdateStopTimes(CancellationToken cancellationToken)
    {
        var stopTimes = await tranzyClient.GetStopTimesAsync(AgencyId);
        await mongoService.UpdateStopTimesAsync(stopTimes, cancellationToken);
    }

    private async Task UpdateStops(CancellationToken cancellationToken)
    {
        var stops = await tranzyClient.GetStopsAsync(AgencyId);
        await mongoService.UpdateStopsAsync(stops, cancellationToken);
    }

    private async Task UpdateShapes(CancellationToken cancellationToken)
    {
        var shapes = await tranzyClient.GetShapeAsync(AgencyId);
        await mongoService.UpdateShapesAsync(shapes, cancellationToken);
    }

    private async Task UpdateRoutes(CancellationToken cancellationToken)
    {
        var routes = await tranzyClient.GetRoutesAsync(AgencyId);
        await mongoService.UpdateRoutesAsync(routes, cancellationToken);
    }
}