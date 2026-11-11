using SampleRack.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SampleRack.Infrastructure.FileStorage;

/// <summary>
/// Stores attachment files on the local (or container-mounted) filesystem under a
/// configurable root directory: <c>{root}/{bucket}/{fileName}</c>. Keys are sanitized
/// to keep writes within the storage root.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configured = configuration["FileStorage:RootPath"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "storage")
            : configured;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> UploadAsync(string bucket, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var path = ResolvePath(bucket, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }
        // The stored key is bucket-relative so DownloadAsync can resolve it later.
        return fileName;
    }

    public Task<Stream> DownloadAsync(string bucket, string fileName, CancellationToken ct)
    {
        var path = ResolvePath(bucket, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException("Stored file not found.", fileName);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string bucket, string fileName, CancellationToken ct)
    {
        var path = ResolvePath(bucket, fileName);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string bucket, string fileName)
    {
        var safeBucket = Sanitize(bucket);
        var safeName = Sanitize(fileName);
        var fullRoot = Path.GetFullPath(_root);
        var path = Path.GetFullPath(Path.Combine(fullRoot, safeBucket, safeName));

        // Guard against path traversal escaping the storage root.
        if (!path.StartsWith(fullRoot, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Resolved storage path is outside the storage root.");

        return path;
    }

    private static string Sanitize(string value)
    {
        var parts = value
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p is not "." and not "..")
            .Select(p => string.Concat(p.Split(Path.GetInvalidFileNameChars())));
        return string.Join(Path.DirectorySeparatorChar, parts);
    }
}
