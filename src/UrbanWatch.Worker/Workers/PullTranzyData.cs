using UrbanWatch.Worker.Clients;
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
        new TimeSpan(4, 0, 0),
        new TimeSpan(9, 0, 0),
    };

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var tasks = new[]
            {
                UpdateDataAsync("Routes", tranzyClient.GetRoutesAsync, mongoService.UpdateRoutesAsync, cancellationToken),
                UpdateDataAsync("Shapes", tranzyClient.GetShapeAsync, mongoService.UpdateShapesAsync, cancellationToken),
                UpdateDataAsync("Stops", tranzyClient.GetStopsAsync, mongoService.UpdateStopsAsync, cancellationToken),
                UpdateDataAsync("StopTimes", tranzyClient.GetStopTimesAsync, mongoService.UpdateStopTimesAsync, cancellationToken),
                UpdateDataAsync("Trips", tranzyClient.GetTripsAsync, mongoService.UpdateTripsAsync, cancellationToken)
            };

            await Task.WhenAll(tasks);
            var delay = timeWindowHelper.GetDelay(_timesVar);

            logger.LogInformation("Delaying for {Delay} before next data pull.", delay);
            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task UpdateDataAsync<T>(
        string dataType,
        Func<string, Task<T>> fetchFunc,
        Func<T, CancellationToken, Task> updateFunc,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Fetching {DataType} data for AgencyId={AgencyId}.", dataType, AgencyId);
            var data = await fetchFunc(AgencyId);
            logger.LogInformation("Fetched {DataType} data successfully. Updating database...", dataType);

            await updateFunc(data, cancellationToken);
            logger.LogInformation("{DataType} data updated successfully.", dataType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating {DataType} data.", dataType);
        }
    }
}
