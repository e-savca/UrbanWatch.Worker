using System;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using UrbanWatch.Worker.Helpers;
using UrbanWatch.Worker.Interfaces;
using Xunit;

namespace UrbanWatch.Worker.Tests;

[TestSubject(typeof(TimeWindowHelper))]
public class TimeWindowHelperTest
{
    private readonly TimeWindowHelper timeWindowHelper;

    public TimeWindowHelperTest()
    {
        var envManager = new Mock<IEnvManager>();
        envManager.SetupAllProperties();
        timeWindowHelper = new TimeWindowHelper(envManager.Object);
    }

[Fact]
public void GetDelay_SingleFutureTime_ReturnsCorrectDelay()
{
    var now = DateTime.UtcNow.TimeOfDay;
    var today = DateTime.UtcNow.Date;
    var futureTime = now + TimeSpan.FromMinutes(10);
    var expected = TimeSpan.FromMinutes(10).TotalMinutes;

    var delay = timeWindowHelper.GetDelay(futureTime);

    Assert.True(delay.TotalMinutes >= 9.9 && delay.TotalMinutes <= 10.1);
    Assert.Equal(expected, Math.Round(delay.TotalMinutes, MidpointRounding.AwayFromZero));
}

[Fact]
public void GetDelay_SinglePastTime_ReturnsDelayForTomorrow()
{
    var now = DateTime.UtcNow.TimeOfDay;
    var pastTime = now - TimeSpan.FromMinutes(10); 

    var delay = timeWindowHelper.GetDelay(pastTime);
    
    var expectedDelay = (TimeSpan.FromHours(24) - now) + pastTime;

    Assert.True(Math.Abs((delay - expectedDelay).TotalMinutes) <= 0.1);
}

[Fact]
public void GetDelay_MultipleTimes_ReturnsMinimumValidDelay()
{
    var now = DateTime.UtcNow.TimeOfDay;

    var expected = TimeSpan.FromMinutes(5).TotalMinutes;

    var times = new[]
    {
        now - TimeSpan.FromMinutes(5), 
        now + TimeSpan.FromMinutes(15),
        now + TimeSpan.FromMinutes(5), 
        now + TimeSpan.FromMinutes(30)
    };

    var delay = timeWindowHelper.GetDelay(times);

    Assert.True(delay.TotalMinutes >= 4.9 && delay.TotalMinutes <= 5.1);
    Assert.Equal(expected, Math.Round(delay.TotalMinutes, MidpointRounding.AwayFromZero));
}

[Fact]
public void GetDelay_AllPastTimes_ReturnsDelayForTomorrowFirstTime()
{
    var now = DateTime.UtcNow.TimeOfDay;

    var times = new[]
    {
        now - TimeSpan.FromMinutes(5),
        now - TimeSpan.FromMinutes(10)
    };

    var firstTomorrow = times.Min();
    var expectedDelay = (TimeSpan.FromHours(24) - now) + firstTomorrow;

    var delay = timeWindowHelper.GetDelay(times);

    Assert.True(Math.Abs((delay - expectedDelay).TotalMinutes) <= 0.1);
}

[Fact]
public void GetDelay_NoTimes_ThrowsException()
{
    Assert.Throws<InvalidOperationException>(() => timeWindowHelper.GetDelay());
}

}