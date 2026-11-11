using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking;
using SampleRack.Application.SampleTracking.Commands.RegisterSample;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.ValueObjects;
using NSubstitute;

namespace SampleRack.Application.Tests.SampleTracking;

public class RegisterSampleHandlerTests
{
    private static readonly TenantId Tenant = TenantId.From(Guid.NewGuid());

    private readonly ISampleRepository _samples = Substitute.For<ISampleRepository>();
    private readonly IRackLocationRepository _locations = Substitute.For<IRackLocationRepository>();
    private readonly ISampleTrackingSettingsRepository _settings = Substitute.For<ISampleTrackingSettingsRepository>();
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly ITenantContext _tenant = Substitute.For<ITenantContext>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly IClockService _clock = Substitute.For<IClockService>();

    private RegisterSampleCommandHandler CreateHandler()
    {
        _tenant.CurrentTenantId.Returns(Tenant);
        _user.Email.Returns("qc@berex.com");
        _clock.UtcNow.Returns(new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        _settings.GetForTenantAsync(Arg.Any<CancellationToken>()).Returns((SampleTrackingSettings?)null);
        return new RegisterSampleCommandHandler(_samples, _locations, _settings, _notifications, _tenant, _user, _clock);
    }

    private static RackLocation ActiveLocation(Guid id)
    {
        var loc = RackLocation.Create(id, Tenant, "A1-02-03", "A1", 2, 3, null);
        return loc;
    }

    private static RegisterSampleCommand Command(Guid locationId) => new(
        locationId, "Bearing housing", "ITEM-001", "Castings", null, null, null, null,
        "ABC Supplier", null, null, null, null,
        new DateOnly(2026, 6, 1), null, 2, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task Generates_sequential_sample_code_for_the_year()
    {
        var handler = CreateHandler();
        var locId = Guid.NewGuid();
        _locations.GetByIdAsync(locId, Arg.Any<CancellationToken>()).Returns(ActiveLocation(locId));
        _samples.GetMaxSequenceForYearAsync("SMP", 2026, Arg.Any<CancellationToken>()).Returns(6);
        _samples.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await handler.Handle(Command(locId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("SMP-2026-0007", result.Value.SampleCode);
        await _samples.Received(1).AddAsync(Arg.Any<Sample>(), Arg.Any<CancellationToken>());
        await _notifications.Received(1).AddAsync(Arg.Any<SampleRack.Domain.SampleTracking.Entities.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_registration_into_inactive_location()
    {
        var handler = CreateHandler();
        var locId = Guid.NewGuid();
        var loc = ActiveLocation(locId);
        loc.Deactivate();
        _locations.GetByIdAsync(locId, Arg.Any<CancellationToken>()).Returns(loc);

        var result = await handler.Handle(Command(locId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleTrackingErrors.LocationInactive, result.Error);
    }

    [Fact]
    public async Task Rejects_registration_when_location_missing()
    {
        var handler = CreateHandler();
        var locId = Guid.NewGuid();
        _locations.GetByIdAsync(locId, Arg.Any<CancellationToken>()).Returns((RackLocation?)null);

        var result = await handler.Handle(Command(locId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleTrackingErrors.LocationNotFound, result.Error);
    }
}
