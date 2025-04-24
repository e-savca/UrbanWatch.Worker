using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker;

public class TimeWindowHelper(IEnvManager envManager)
{
    private IEnvManager EnvManager { get; } = envManager;
    
    public TimeSpan GetDelay(TimeSpan startWindow, TimeSpan endWindow)
    {
        var now = DateTime.UtcNow.TimeOfDay;

        if (!IsInWindow(startWindow, endWindow))
        {
            TimeSpan delay;
            if (startWindow <= endWindow)
            {
                delay = startWindow - now;
            }
            else
            {
                if (now < startWindow && now > endWindow)
                {
                    delay = startWindow - now;
                }
                else
                {
                    delay = (TimeSpan.FromHours(24) - now) + startWindow;
                }
            }

            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            return delay;

        }
        
        return EnvManager.IsDevelopment()
            ? TimeSpan.FromMilliseconds(30000)
            : TimeSpan.FromMicroseconds(5000);
    }

    public TimeSpan GetDelay(params TimeSpan[] times)
    {
        var now = DateTime.UtcNow.TimeOfDay;

        var delays = times
            .Select(t => t - now)
            .Where(d => d >= TimeSpan.Zero)
            .ToList();

        if (delays.Any())
        {
            return delays.Min();
        }
        else
        {
            var firstTomorrow = times.Min();
            var delayUntilTomorrow = (TimeSpan.FromHours(24) - now) + firstTomorrow;
            return delayUntilTomorrow;
        }
    }


    private bool IsInWindow(TimeSpan startWindow, TimeSpan endWindow)
    {
        var now = DateTime.UtcNow.TimeOfDay;
        if (startWindow <= endWindow)
        {
            return now > startWindow && now < endWindow;
        }
        return now >= startWindow || now <= endWindow;
    }
}