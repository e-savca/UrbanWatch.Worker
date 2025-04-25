using System.Security.Cryptography;
using System.Text;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Infrastructure.HttpClients;
using UrbanWatch.Worker.Infrastructure.Mongo.Documents;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;

namespace UrbanWatch.Worker.Workers;

public class FetchVehiclesWorker(
    TranzyClient client,
    CacheService cacheService,
    VehicleHistoryService vehicleHistoryService,
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
        _logger.LogInformation("FetchVehiclesWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Fetching vehicles for AgencyId: {AgencyId}", AgencyId);

            var vehicles = await client.GetVehiclesAsync(AgencyId);
            _logger.LogInformation("Fetched {Count} vehicles.", vehicles.Count);

            if (_vehiclesHashCache.Count != 0)
            {
                _logger.LogInformation("Checking for changes in vehicle data...");

                var rnd = new Random();
                var sample = vehicles.OrderBy(_ => rnd.Next()).Take(20).ToList();

                bool anyChanged = sample.Any(s =>
                {
                    var currentHash = ComputeVehicleHash(s);
                    if (s.VehicleId == null)
                    {
                        _logger.LogWarning("Vehicle with null VehicleId found in sample.");
                        return false;
                    }

                    var exists = _vehiclesHashCache.TryGetValue(s.VehicleId, out var hash);
                    var changed = !exists || hash != currentHash;

                    if (changed)
                    {
                        _logger.LogInformation("Change detected for VehicleId: {VehicleId}", s.VehicleId);
                    }

                    return changed;
                });

                if (anyChanged)
                {
                    _logger.LogInformation("Changes detected, saving batch and updating cache...");

                    var tasks = new[]
                    {
                    vehicleHistoryService.SaveBatchAsync(vehicles, stoppingToken),
                    cacheService.CacheVehiclesAsync(vehicles)
                };

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("Batch saved and cache updated.");
                    AddOrUpdateVehiclesHash(vehicles);
                    _logger.LogInformation("Vehicle hashes updated.");
                }
                else
                {
                    _logger.LogInformation("No changes detected in the sampled vehicles.");
                }
            }
            else
            {
                _logger.LogInformation("First run: saving initial batch and caching vehicles...");

                var tasks = new[]
                {
                vehicleHistoryService.SaveBatchAsync(vehicles, stoppingToken),
                cacheService.CacheVehiclesAsync(vehicles)
            };

                await Task.WhenAll(tasks);

                _logger.LogInformation("Initial batch saved and cache initialized.");
                AddOrUpdateVehiclesHash(vehicles);
                _logger.LogInformation("Initial vehicle hashes stored.");
            }

            var delay = TimeWindowHelper.GetDelay(_startWindow, _endWindow);
            _logger.LogInformation("Delaying for {Delay} before next fetch cycle.", delay);

            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("FetchVehiclesWorker is stopping.");
    }

    private void AddOrUpdateVehiclesHash(List<Vehicle> vehicles)
    {
        foreach (var v in vehicles)
        {
            if (!string.IsNullOrWhiteSpace(v.VehicleId))
            {
                var hash = ComputeVehicleHash(v);
                _vehiclesHashCache[v.VehicleId] = hash;
                _logger.LogDebug("Updated hash for VehicleId: {VehicleId}", v.VehicleId);
            }
        }
    }

    private string ComputeVehicleHash(Vehicle vehicle)
    {
        var input = $"{vehicle.Latitude}_{vehicle.Longitude}_{vehicle.Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var result = Convert.ToBase64String(bytes);

        _logger.LogDebug("Computed hash for VehicleId: {VehicleId}: {Hash}", vehicle.VehicleId, result);

        return result;
    }

}