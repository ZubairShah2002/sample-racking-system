using SampleRack.SharedKernel.Abstractions;

namespace SampleRack.SharedKernel.ValueObjects;

/// <summary>
/// Represents a file attachment with metadata: reference path, MIME type, size, and content hash.
/// </summary>
public sealed class Attachment : ValueObject
{
    /// <summary>
    /// The reference path or URI to the file in storage.
    /// </summary>
    public string FileReference { get; }

    /// <summary>
    /// The MIME type of the file (e.g., "application/pdf").
    /// </summary>
    public string MimeType { get; }

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public long SizeInBytes { get; }

    /// <summary>
    /// The SHA-256 content hash for integrity verification.
    /// </summary>
    public string ContentHash { get; }

    private Attachment(string fileReference, string mimeType, long sizeInBytes, string contentHash)
    {
        FileReference = fileReference;
        MimeType = mimeType;
        SizeInBytes = sizeInBytes;
        ContentHash = contentHash;
    }

    /// <summary>
    /// Creates a new <see cref="Attachment"/> with validation.
    /// </summary>
    public static Attachment Create(string fileReference, string mimeType, long sizeInBytes, string contentHash)
    {
        if (string.IsNullOrWhiteSpace(fileReference))
            throw new ArgumentException("File reference is required.", nameof(fileReference));

        if (string.IsNullOrWhiteSpace(mimeType))
            throw new ArgumentException("MIME type is required.", nameof(mimeType));

        if (sizeInBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "File size cannot be negative.");

        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash is required.", nameof(contentHash));

        return new Attachment(
            fileReference.Trim(),
            mimeType.Trim().ToLowerInvariant(),
            sizeInBytes,
            contentHash.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FileReference;
        yield return MimeType;
        yield return SizeInBytes;
        yield return ContentHash;
    }

    public override string ToString() => $"{FileReference} ({MimeType}, {SizeInBytes} bytes)";
}
