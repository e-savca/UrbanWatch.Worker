
using UrbanWatch.Worker.Services;

namespace UrbanWatch.Worker.Workers;

/// <summary>
/// Responsible for cleaning up the vehicle history data in the database.
/// </summary>
public class CleanupVehiclesWorker(CleanupVehiclesService cleanupVehiclesService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await cleanupVehiclesService.ClearAsync(TimeSpan.FromHours(2));
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }   
}
