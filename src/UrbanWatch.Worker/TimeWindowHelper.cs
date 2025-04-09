using UrbanWatch.Worker.ConfigManager;

namespace UrbanWatch.Worker;

public class TimeWindowHelper(EnvManager envManager)
{
    private EnvManager EnvManager { get; } = envManager;
    
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