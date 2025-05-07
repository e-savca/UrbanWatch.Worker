using System.Security.Cryptography;
using System.Text;
using UrbanWatch.Worker.DTOs;
using UrbanWatch.Worker.HttpClients;

namespace UrbanWatch.Worker.Services;

public class FetchVehicleService(
    TranzyClient tranzyClient,
    UrbanWatchClient urbanWatchClient,
    ILogger<FetchVehicleService> logger
)
{
    private const string AgencyId = "4";
    private readonly Dictionary<string, string> _vehiclesHashCache = new Dictionary<string, string>();

    public async Task Run()
    {
        logger.LogInformation("Fetching vehicles for AgencyId: {AgencyId}", AgencyId);

        var vehicles = await tranzyClient.GetVehiclesAsync(AgencyId);
        logger.LogInformation("Fetched {Count} vehicles.", vehicles.Count);

        if (_vehiclesHashCache.Count == 0)
        {
            await PushVehicles(vehicles);
            return;
        }
        
        var anyChange = CheckVehiclesForChanges(vehicles);
        if (!anyChange)
        {
            logger.LogInformation("No changes detected in the sampled vehicles.");
            return;
        }
        
        await PushVehicles(vehicles);
    }

    private async Task PushVehicles(List<Vehicle> vehicles)
    {
        logger.LogInformation("Changes detected, saving batch and updating cache...");

        var snapshot = GetVehicleSnapshot(vehicles);

        try
        {
            AddOrUpdateVehiclesHash(vehicles);
            logger.LogInformation("Vehicle hashes updated.");
            await urbanWatchClient.SendVehicles(snapshot);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while saving vehicles.");
            throw;
        }
        
        logger.LogInformation("Pushed vehicles.");
        
        // var tasks = new[]
        // {
        //     // vehicleHistoryService.SaveBatchAsync(vehicles, stoppingToken),
        //     // cacheService.CacheVehiclesAsync(vehicles)
        // };
        //
        // await Task.WhenAll(tasks);

    }
    private bool CheckVehiclesForChanges(List<Vehicle> vehicles)
    {
        logger.LogInformation("Checking for changes in vehicle data...");

        var rnd = new Random();
        var sample = vehicles.OrderBy(_ => rnd.Next()).Take(20).ToList();

        bool anyChanged = sample.Any(s =>
        {
            var currentHash = ComputeVehicleHash(s);
            if (s.VehicleId == null)
            {
                logger.LogWarning("Vehicle with null VehicleId found in sample.");
                return false;
            }

            var exists = _vehiclesHashCache.TryGetValue(s.VehicleId, out var hash);
            var changed = !exists || hash != currentHash;

            if (changed)
            {
                logger.LogInformation("Change detected for VehicleId: {VehicleId}", s.VehicleId);
            }

            return changed;
        });

        return anyChanged;
    }

    private VehicleSnapshot GetVehicleSnapshot(List<Vehicle> vehicles)
    {
        return new VehicleSnapshot
        {
            Timestamp = DateTime.UtcNow,
            Vehicles = vehicles
        };
    }
    
    private void AddOrUpdateVehiclesHash(List<Vehicle> vehicles)
    {
        foreach (var v in vehicles)
        {
            if (!string.IsNullOrWhiteSpace(v.VehicleId))
            {
                var hash = ComputeVehicleHash(v);
                _vehiclesHashCache[v.VehicleId] = hash;
                logger.LogDebug("Updated hash for VehicleId: {VehicleId}", v.VehicleId);
            }
        }
    }

    private string ComputeVehicleHash(Vehicle vehicle)
    {
        var input = $"{vehicle.Latitude}_{vehicle.Longitude}_{vehicle.Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var result = Convert.ToBase64String(bytes);

        logger.LogDebug("Computed hash for VehicleId: {VehicleId}: {Hash}", vehicle.VehicleId, result);

        return result;
    }
}