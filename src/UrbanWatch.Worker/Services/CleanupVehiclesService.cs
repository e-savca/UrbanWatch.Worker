using UrbanWatch.Worker.HttpClients;

namespace UrbanWatch.Worker.Services;

public class CleanupVehiclesService(
    UrbanWatchClient urbanWatchClient,
    ILogger<CleanupVehiclesService> logger
    )
{
    public async Task ClearAsync(TimeSpan timeSpan)
    {
        try
        {
            await urbanWatchClient.ClearVehiclesAsync(timeSpan);
            logger.LogInformation("Clear vehicles successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }
}