// using System;
// using UrbanWatch.Worker.Services;
//
// namespace UrbanWatch.Worker.Workers;
//
// /// <summary>
// /// Responsible for cleaning up the vehicle history data in the database.
// /// </summary>
// /// <param name="vehicleHistoryService"></param>
// public class CleanupVehiclesLive(VehicleHistoryService vehicleHistoryService) : BackgroundService
// {
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await vehicleHistoryService.ClearAsync(TimeSpan.FromHours(2), stoppingToken);
//             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//         }
//     }   
// }
