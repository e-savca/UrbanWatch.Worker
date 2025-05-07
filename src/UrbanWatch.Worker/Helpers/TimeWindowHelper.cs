using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker.Helpers;

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
            ? TimeSpan.FromSeconds(30)
            : TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Calculates the delay until the next scheduled <see cref="TimeSpan"/> in the provided array,
    /// relative to the current UTC time of day.
    /// </summary>
    /// <param name="times">
    /// An array of <see cref="TimeSpan"/> values representing scheduled times within a day.
    /// Each value must have a <c>Days</c> component of 0 (i.e., represent only hours, minutes, and seconds).
    /// </param>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the time to wait until the next scheduled time.
    /// If all scheduled times have passed for today, the delay will include the remaining time until midnight plus the time to the first scheduled time tomorrow.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any of the provided <paramref name="times"/> contains a <c>Days</c> component greater than 0.
    /// </exception>
    public TimeSpan GetDelay(params TimeSpan[] times)
    {
        if (times.Any(time => time.Days > 0))
        {
            throw new InvalidOperationException(
                "Invalid TimeSpan detected: all scheduled times must be within a single day (Days component must be 0)."
            );
        }

        var now = DateTime.UtcNow.TimeOfDay;

        var futureTimesToday = times
            .Select(t => t.TotalMinutes)
            .Where(minutes => minutes >= now.TotalMinutes)
            .ToList();

        if (futureTimesToday.Any())
        {
            var nextTime = futureTimesToday.Min();
            var delayMinutes = nextTime - now.TotalMinutes;
            return TimeSpan.FromMinutes(delayMinutes);
        }
        else
        {
            var firstTimeTomorrow = times.Min();
            var remainingToday = TimeSpan.FromHours(24) - now;
            var delayUntilTomorrow = remainingToday + firstTimeTomorrow;
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