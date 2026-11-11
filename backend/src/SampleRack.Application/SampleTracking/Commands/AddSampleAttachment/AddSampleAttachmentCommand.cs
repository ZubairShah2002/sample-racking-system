using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Application.SampleTracking.DTOs;
using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.Application.Interfaces;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.AddSampleAttachment;

/// <summary>Records metadata for a document/photo already uploaded to object storage.</summary>
public sealed record AddSampleAttachmentCommand(
    Guid SampleId,
    string FileName,
    string FileRef,
    string? ContentType,
    long? SizeBytes,
    string Category) : ICommand<SampleAttachmentDto>;

public sealed class AddSampleAttachmentCommandValidator : AbstractValidator<AddSampleAttachmentCommand>
{
    public AddSampleAttachmentCommandValidator()
    {
        RuleFor(x => x.SampleId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.FileRef).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Category).NotEmpty();
    }
}

internal sealed class AddSampleAttachmentCommandHandler : ICommandHandler<AddSampleAttachmentCommand, SampleAttachmentDto>
{
    private readonly ISampleRepository _samples;
    private readonly ICurrentUserService _currentUser;
    private readonly IClockService _clock;

    public AddSampleAttachmentCommandHandler(ISampleRepository samples, ICurrentUserService currentUser, IClockService clock)
    {
        _samples = samples;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SampleAttachmentDto>> Handle(AddSampleAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AttachmentCategory>(request.Category, ignoreCase: true, out var category))
            return SampleTrackingErrors.InvalidAttachmentCategory;

        var sample = await _samples.GetFullDetailAsync(request.SampleId, cancellationToken);
        if (sample is null)
            return SampleTrackingErrors.SampleNotFound;

        var now = _clock.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.Email) ? "system" : _currentUser.Email;

        var attachment = sample.AddAttachment(
            request.FileName, request.FileRef, request.ContentType, request.SizeBytes, category, actor, now);

        await _samples.UpdateAsync(sample, cancellationToken);

        return new SampleAttachmentDto(
            attachment.Id, attachment.FileName, attachment.FileRef, attachment.ContentType,
            attachment.SizeBytes, attachment.Category, attachment.UploadedBy, attachment.UploadedAt);
    }
}
