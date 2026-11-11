using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.Interfaces;
using SampleRack.Application.SampleTracking.Common;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.RejectDisposal;

/// <summary>Rejects a pending disposal: the sample remains stored and is placed On Hold.</summary>
public sealed record RejectDisposalCommand(Guid Id, string? Remarks) : ICommand<SampleDetailDto>;

public sealed class RejectDisposalCommandValidator : AbstractValidator<RejectDisposalCommand>
{
    public RejectDisposalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

internal sealed class RejectDisposalCommandHandler : ICommandHandler<RejectDisposalCommand, SampleDetailDto>
{
    private readonly ISampleRepository _samples;
    private readonly IRackLocationRepository _locations;
    private readonly ISampleTrackingSettingsRepository _settings;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public RejectDisposalCommandHandler(
        ISampleRepository samples, IRackLocationRepository locations, ISampleTrackingSettingsRepository settings,
        ICurrentUserService currentUser, IClockService clock)
    {
        _samples = samples;
        _locations = locations;
        _settings = settings;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleDetailDto>> Handle(RejectDisposalCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.Id, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;
        if (sample.Disposal is null || sample.Disposal.ApprovalStatus != "Pending")
            return SampleTrackingErrors.NoPendingDisposal;

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        sample.RejectDisposal(actor, request.Remarks, now);
        await _samples.UpdateAsync(sample, cancellationToken);

        var settings = await _settings.GetForTenantAsync(cancellationToken);
        var today = SampleTrackingTime.Today(settings, _clock);
        var thresholds = SampleTrackingTime.Thresholds(settings);
        var locationCode = sample.RackLocationId.HasValue
            ? (await _locations.GetByIdAsync(sample.RackLocationId.Value, cancellationToken))?.Code
            : null;

        return SampleTrackingMapper.ToDetailDto(sample, today, thresholds, locationCode);
    }
}
