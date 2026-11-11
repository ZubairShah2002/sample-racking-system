using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Entities;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.UpdateSample;

/// <summary>Edits a sample's descriptive fields. Previous values are preserved in the
/// shared audit trail (captured automatically by the audit interceptor).</summary>
public sealed record UpdateSampleCommand(
    Guid Id,
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

public sealed class UpdateSampleCommandValidator : AbstractValidator<UpdateSampleCommand>
{
    public UpdateSampleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SampleName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
    }
}

internal sealed class UpdateSampleCommandHandler : ICommandHandler<UpdateSampleCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly IClockService _clock;

    public UpdateSampleCommandHandler(
        ISampleRepository samples,
        IRackLocationRepository locations,
        ISampleTrackingSettingsRepository settings,
        IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(UpdateSampleCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.IsDisposed)
            return SampleTrackingErrors.SampleDisposed;

        var profile = new SampleProfile(
            request.SampleName, request.ItemCode, request.ProductCategory, request.Model, request.Size,
            request.Variant, request.MaterialType, request.SupplierName, request.SupplierCode,
            request.PurchaseOrderNumber, request.GrnNumber, request.SupplierReference,
            request.ReceivedDate, request.ReceivedTime, request.ReceivedQuantity, request.ReceivedBy,
            request.Department, request.SampleOwner, request.Description, request.Purpose,
            request.TestingRequirement, request.RequiredTestingType, request.StorageRemarks);

        sample.UpdateDetails(profile);
        await _samples.UpdateAsync(sample, cancellationToken);

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = await ResolveLocationCodeAsync(sample.RackLocationId, cancellationToken);

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }

    private async Task<string?> ResolveLocationCodeAsync(Guid? locationId, CancellationToken ct)
    {
        if (!locationId.HasValue) return null;
        var location = await _locations.GetByIdAsync(locationId.Value, ct);
        return location?.Code;
    }
}
