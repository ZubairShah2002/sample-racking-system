namespace SampleRack.Application.Interfaces;

/// <summary>
/// Abstraction for storing and retrieving sample attachment files. The default
/// implementation stores files on the local/mounted filesystem; it can be swapped
/// for S3/MinIO/Azure Blob without touching callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Stores a file and returns the storage key/path to persist against the sample.</summary>
    Task<string> UploadAsync(string bucket, string fileName, Stream content, string contentType, CancellationToken ct);

    /// <summary>Opens a readable stream for a previously stored file.</summary>
    Task<Stream> DownloadAsync(string bucket, string fileName, CancellationToken ct);

    /// <summary>Deletes a stored file (no-op if it does not exist).</summary>
    Task DeleteAsync(string bucket, string fileName, CancellationToken ct);
}
