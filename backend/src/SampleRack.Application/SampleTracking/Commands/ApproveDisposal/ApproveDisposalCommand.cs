using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.ApproveDisposal;

/// <summary>Approves a pending disposal: marks the sample Disposed, releases the rack
/// location, and retains all historical records.</summary>
public sealed record ApproveDisposalCommand(Guid Id, DateOnly? DisposalDate, string? Remarks) : ICommand<SampleDetailDto>;

public sealed class ApproveDisposalCommandValidator : AbstractValidator<ApproveDisposalCommand>
{
    public ApproveDisposalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

internal sealed class ApproveDisposalCommandHandler : ICommandHandler<ApproveDisposalCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public ApproveDisposalCommandHandler(
        ISampleRepository samples, ISampleTrackingSettingsRepository settings,
        ICurrentUserService currentUser, IClockService clock)
    {
        _samples = samples;
        _settings = settings;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(ApproveDisposalCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.Disposal is null || sample.Disposal.ApprovalStatus != "Pending")
            return SampleTrackingErrors.NoPendingDisposal;

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        sample.ApproveDisposal(actor, request.DisposalDate ?? today, request.Remarks, now);
        await _samples.UpdateAsync(sample, cancellationToken);

        // Location released on disposal: the sample no longer has a current location code.
        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode: null);
    }
}
