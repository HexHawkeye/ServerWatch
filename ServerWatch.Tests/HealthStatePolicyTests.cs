using ServerWatch.Core.Monitoring;
using Xunit;

namespace ServerWatch.Tests;

public class HealthStatePolicyTests
{
    [Fact]
    public void Success_is_healthy_and_resets_failures()
    {
        var result = HealthStatePolicy.Evaluate(true, 2, 3);

        Assert.Equal("Healthy", result.Status);
        Assert.Equal(0, result.Failures);
    }

    [Fact]
    public void Failure_below_threshold_is_warning()
    {
        var result = HealthStatePolicy.Evaluate(false, 0, 3);

        Assert.Equal("Warning", result.Status);
        Assert.Equal(1, result.Failures);
    }

    [Fact]
    public void Failure_at_threshold_is_unhealthy()
    {
        var result = HealthStatePolicy.Evaluate(false, 2, 3);

        Assert.Equal("Unhealthy", result.Status);
        Assert.Equal(3, result.Failures);
    }

    [Fact]
    public void Alerts_only_fire_on_transitions()
    {
        Assert.True(
            HealthStatePolicy.IsFailureAlert("Warning", "Unhealthy"));

        Assert.True(
            HealthStatePolicy.IsRecoveryAlert("Unhealthy", "Healthy"));

        Assert.False(
            HealthStatePolicy.IsFailureAlert("Unhealthy", "Unhealthy"));
    }
}