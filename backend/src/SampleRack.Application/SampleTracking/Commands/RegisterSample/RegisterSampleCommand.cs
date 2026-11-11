using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;
using DomainNotification = SampleRack.Domain.SampleTracking.Entities.Notification;

namespace SampleRack.Application.SampleTracking.Commands.RegisterSample;

/// <summary>Registers a new sample into a rack location with an auto-generated unique code.</summary>
public sealed record RegisterSampleCommand(
    Guid RackLocationId,
    string SampleName,
    string? ItemCode,
    string? ProductCategory,
    string? Model,
    string? Size,
    string? Variant,
    string? MaterialType,
    string? SupplierName,
    string? SupplierCode,
    string? PurchaseOrderNumber,
    string? GrnNumber,
    string? SupplierReference,
    DateOnly ReceivedDate,
    TimeOnly? ReceivedTime,
    decimal ReceivedQuantity,
    string? ReceivedBy,
    string? Department,
    string? SampleOwner,
    string? Description,
    string? Purpose,
    string? TestingRequirement,
    string? RequiredTestingType,
    string? StorageRemarks) : ICommand<SampleDetailDto>;

public sealed class RegisterSampleCommandValidator : AbstractValidator<RegisterSampleCommand>
{
    public RegisterSampleCommandValidator()
    {
        RuleFor(x => x.RackLocationId).NotEmpty();
        RuleFor(x => x.SampleName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ItemCode).MaximumLength(100);
        RuleFor(x => x.ProductCategory).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.Size).MaximumLength(100);
        RuleFor(x => x.Variant).MaximumLength(100);
        RuleFor(x => x.MaterialType).MaximumLength(100);
        RuleFor(x => x.SupplierName).MaximumLength(200);
        RuleFor(x => x.SupplierCode).MaximumLength(100);
        RuleFor(x => x.PurchaseOrderNumber).MaximumLength(100);
        RuleFor(x => x.GrnNumber).MaximumLength(100);
        RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
        RuleFor(x => x.ReceivedBy).MaximumLength(150);
        RuleFor(x => x.Department).MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.StorageRemarks).MaximumLength(1000);
    }
}

internal sealed class RegisterSampleCommandHandler : ICommandHandler<RegisterSampleCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly INotificationRepository _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public RegisterSampleCommandHandler(
        ISampleRepository samples,
        IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings,
        INotificationRepository notifications,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _notifications = notifications;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(RegisterSampleCommand request, CancellationToken cancellationToken)
    {
        var location = await _locations.GetByIdAsync(request.RackLocationId, cancellationToken);
        if (location is null)
            return SampleTrackingErrors.LocationNotFound;
        if (!location.IsActive)
            return SampleTrackingErrors.LocationInactive;

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var prefix = string.IsNullOrWhiteSpace(settings?.SampleIdPrefix) ? "SMP" : settings!.SampleIdPrefix;

        var code = await GenerateUniqueCodeAsync(prefix, today.Year, cancellationToken);
        var now = _clock.UtcNow;
        var actor = Actor();

        var profile = new SampleProfile(
            request.SampleName, request.ItemCode, request.ProductCategory, request.Model, request.Size,
            request.Variant, request.MaterialType, request.SupplierName, request.SupplierCode,
            request.PurchaseOrderNumber, request.GrnNumber, request.SupplierReference,
            request.ReceivedDate, request.ReceivedTime, request.ReceivedQuantity, request.ReceivedBy,
            request.Department, request.SampleOwner, request.Description, request.Purpose,
            request.TestingRequirement, request.RequiredTestingType, request.StorageRemarks);

        var sample = Sample.Register(
            Guid.NewGuid(), _tenantContext.CurrentTenantId, code, location.Id, profile, actor, now);

        await _samples.AddAsync(sample, cancellationToken);

        await _notifications.AddAsync(DomainNotification.Create(
            Guid.NewGuid(), _tenantContext.CurrentTenantId,
            NotificationType.SampleRegistered, NotificationSeverity.Info,
            "New sample registered",
            $"Sample {code} ({sample.SampleName}) registered in location {location.Code}.",
            sample.Id, code, dedupeKey: null, now), cancellationToken);

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, location.Code);
    }

    private async Task<string> GenerateUniqueCodeAsync(string prefix, int year, CancellationToken ct)
    {
        var seq = await _samples.GetMaxSequenceForYearAsync(prefix, year, ct) + 1;
        string code;
        do
        {
            code = $"{prefix}-{year}-{seq:D4}";
            seq++;
        } while (await _samples.CodeExistsAsync(code, ct));
        return code;
    }

    private string Actor()
        => string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;
}
