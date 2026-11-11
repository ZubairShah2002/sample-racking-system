using SampleRack.Domain.SampleTracking.Enums;
using SampleRack.SharedKernel.Abstractions;
using SampleRack.SharedKernel.Exceptions;
using SampleRack.SharedKernel.ValueObjects;

namespace SampleRack.Domain.SampleTracking.Entities;

/// <summary>
/// A document or photo attached to a sample. The binary content is stored in
/// object storage; this entity holds the reference (object key) and metadata.
/// </summary>
public sealed class SampleAttachment : Entity<Guid>
{
    public Guid SampleId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileRef { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public long? SizeBytes { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string UploadedBy { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    private SampleAttachment() { }

    internal static SampleAttachment Create(
        Guid id,
        TenantId tenantId,
        Guid sampleId,
        string fileName,
        string fileRef,
        string? contentType,
        long? sizeBytes,
        AttachmentCategory category,
        string uploadedBy,
        DateTime uploadedAt)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Attachment file name is required.");
        if (string.IsNullOrWhiteSpace(fileRef))
            throw new DomainException("Attachment file reference is required.");

        return new SampleAttachment
        {
            Id = id,
            TenantId = tenantId,
            SampleId = sampleId,
            FileName = fileName.Trim(),
            FileRef = fileRef.Trim(),
            ContentType = contentType?.Trim(),
            SizeBytes = sizeBytes,
            Category = category.ToString(),
            UploadedBy = string.IsNullOrWhiteSpace(uploadedBy) ? "system" : uploadedBy.Trim(),
            UploadedAt = uploadedAt
        };
    }
}
