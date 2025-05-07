using System.Security.Cryptography;
using System.Text;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.DTOs;
using UrbanWatch.Worker.Helpers;
using UrbanWatch.Worker.HttpClients;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;

namespace UrbanWatch.Worker.Workers;

public class FetchVehiclesWorker(
    FetchVehicleService fetchVehicleService,
    ILogger<FetchVehiclesWorker> logger,
    TimeWindowHelper timeWindowHelper
    ) : BackgroundService
{
    private readonly TimeSpan _startWindow = new TimeSpan(2, 10, 0);
    private readonly TimeSpan _endWindow = new TimeSpan(21, 30, 0);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FetchVehiclesWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await fetchVehicleService.Run();
            
            var delay = timeWindowHelper.GetDelay(_startWindow, _endWindow);
            logger.LogInformation("Delaying for {Delay} before next fetch cycle.", delay);

            await Task.Delay(delay, stoppingToken);
        }

        logger.LogInformation("FetchVehiclesWorker is stopping.");
    }
}