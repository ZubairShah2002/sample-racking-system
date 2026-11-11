using SampleRack.Domain.SampleTracking;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.ValueObjects;

namespace SampleRack.Domain.Tests.SampleTracking;

public class AgingCalculatorTests
{
    private static readonly AgingThresholds Default = AgingThresholds.Default; // 30 / 60 / 90

    [Theory]
    [InlineData(0, AgingCategory.New)]
    [InlineData(30, AgingCategory.New)]
    [InlineData(31, AgingCategory.Normal)]
    [InlineData(60, AgingCategory.Normal)]
    [InlineData(61, AgingCategory.ApproachingLimit)]
    [InlineData(89, AgingCategory.ApproachingLimit)]
    [InlineData(90, AgingCategory.Overdue)]
    [InlineData(200, AgingCategory.Overdue)]
    public void Classify_uses_configured_boundaries(int days, AgingCategory expected)
    {
        Assert.Equal(expected, AgingCalculator.Classify(days, Default));
    }

    [Fact]
    public void Calculate_reproduces_the_specification_example()
    {
        // Received 2026-08-01, evaluated 2026-11-07 => 98 days, overdue by 8.
        var received = new DateOnly(2026, 8, 1);
        var asOf = new DateOnly(2026, 11, 7);

        var snap = AgingCalculator.Calculate(received, asOf, Default);

        Assert.Equal(98, snap.TotalDaysStored);
        Assert.Equal(AgingCategory.Overdue, snap.Category);
        Assert.Equal(8, snap.DaysOverdue);
        Assert.Equal(-8, snap.DaysRemainingBeforeOverdue);
    }

    [Fact]
    public void Calculate_clamps_future_received_date_to_zero()
    {
        var received = new DateOnly(2026, 12, 1);
        var asOf = new DateOnly(2026, 11, 7);

        var snap = AgingCalculator.Calculate(received, asOf, Default);

        Assert.Equal(0, snap.TotalDaysStored);
        Assert.Equal(0, snap.DaysOverdue);
        Assert.Equal(AgingCategory.New, snap.Category);
    }

    [Fact]
    public void Calculate_reports_days_remaining_before_overdue()
    {
        var snap = AgingCalculator.Calculate(new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 20), Default); // 50 days
        Assert.Equal(40, snap.DaysRemainingBeforeOverdue);
        Assert.Equal(0, snap.DaysOverdue);
        Assert.Equal(AgingCategory.Normal, snap.Category);
    }

    [Fact]
    public void Thresholds_create_forces_increasing_sequence()
    {
        var t = AgingThresholds.Create(60, 40, 30); // out of order
        Assert.True(t.NewMaxDays < t.NormalMaxDays);
        Assert.True(t.NormalMaxDays < t.OverdueDays);
    }

    [Fact]
    public void Thresholds_create_falls_back_to_defaults_for_nonpositive()
    {
        var t = AgingThresholds.Create(null, 0, -5);
        Assert.Equal(AgingThresholds.Default, t);
    }
}
