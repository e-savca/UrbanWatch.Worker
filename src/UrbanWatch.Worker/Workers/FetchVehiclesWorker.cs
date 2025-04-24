using System.Security.Cryptography;
using System.Text;
using UrbanWatch.Worker.Clients;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Documents;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;

namespace UrbanWatch.Worker.Workers;

public class FetchVehiclesWorker(
    TranzyClient client,
    // RedisContext redisContext,
    VehicleHistoryService vehicleHistoryService,
    IEnvManager envManager,
    ILogger<FetchVehiclesWorker> logger,
    TimeWindowHelper timeWindowHelper
    ) : BackgroundService
{
    private const string AgencyId = "4";
    private readonly TimeSpan _startWindow = new TimeSpan(2, 10, 0);
    private readonly TimeSpan _endWindow = new TimeSpan(21, 30, 0);

    private TimeWindowHelper TimeWindowHelper { get; } = timeWindowHelper;
    private readonly ILogger<FetchVehiclesWorker> _logger = logger;

    private readonly Dictionary<string, string> _vehiclesHashCache = new Dictionary<string, string>();


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var vehicles = await client.GetVehiclesAsync(AgencyId);

            if (_vehiclesHashCache.Count != 0)
            {
                var rnd = new Random();
                var sample = vehicles.OrderBy(_ => rnd.Next()).Take(20).ToList();

                bool anyChanged = sample.Any(s =>
                {
                    var currentHash = ComputeVehicleHash(s);
                    if (s.VehicleId == null) return false;
                    return !_vehiclesHashCache.TryGetValue(s.VehicleId, out var hash) || hash != currentHash;
                });
                if (anyChanged)
                {
                    await vehicleHistoryService.SaveBatchAsync(vehicles, stoppingToken);
                    AddOrUpdateVehiclesHash(vehicles);
                }
            }
            else
            {
                AddOrUpdateVehiclesHash(vehicles);
                await vehicleHistoryService.SaveBatchAsync(vehicles, stoppingToken);
            }
            
            var delay = TimeWindowHelper.GetDelay(_startWindow, _endWindow);

            await Task.Delay(delay, stoppingToken);
        }
    }

    private void AddOrUpdateVehiclesHash(List<Vehicle> vehicles)
    {
        foreach (var v in vehicles)
        {
            if (!string.IsNullOrWhiteSpace(v.VehicleId))
                _vehiclesHashCache[v.VehicleId] = ComputeVehicleHash(v);
        }
    }

    private string ComputeVehicleHash(Vehicle vehicle)
    {
        var input = $"{vehicle.Latitude}_{vehicle.Longitude}_{vehicle.Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}