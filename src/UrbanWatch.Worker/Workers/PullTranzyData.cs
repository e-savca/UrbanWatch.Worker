using System;

namespace UrbanWatch.Worker.Workers;

public class PullTranzyData : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken){
        while (!cancellationToken.IsCancellationRequested)
        {

            
            await Task.Delay(TimeSpan.FromHours(12), cancellationToken);
        }
    }

}
