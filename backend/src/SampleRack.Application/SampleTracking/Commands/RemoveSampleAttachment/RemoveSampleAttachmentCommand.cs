using SampleRack.Application.Abstractions.Messaging;
using SampleRack.Domain.SampleTracking.Repositories;
using SampleRack.SharedKernel.Results;
using FluentValidation;

namespace SampleRack.Application.SampleTracking.Commands.RemoveSampleAttachment;

public sealed record RemoveSampleAttachmentCommand(Guid SampleId, Guid AttachmentId) : ICommand;

public sealed class RemoveSampleAttachmentCommandValidator : AbstractValidator<RemoveSampleAttachmentCommand>
{
    public RemoveSampleAttachmentCommandValidator()
    {
        RuleFor(x => x.SampleId).NotEmpty();
        RuleFor(x => x.AttachmentId).NotEmpty();
    }
}

internal sealed class RemoveSampleAttachmentCommandHandler : ICommandHandler<RemoveSampleAttachmentCommand>
{
    private readonly ISampleRepository _samples;

    public RemoveSampleAttachmentCommandHandler(ISampleRepository samples)
    {
        _samples = samples;
    }

    public async Task<Result> Handle(RemoveSampleAttachmentCommand request, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetFullDetailAsync(request.SampleId, cancellationToken);
        if (sample is null)
            return Result.Failure(SampleTrackingErrors.SampleNotFound);

        sample.RemoveAttachment(request.AttachmentId);
        await _samples.UpdateAsync(sample, cancellationToken);
        return Result.Success();
    }
}
