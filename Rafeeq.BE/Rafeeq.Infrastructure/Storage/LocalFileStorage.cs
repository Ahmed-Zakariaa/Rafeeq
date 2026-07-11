using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rafeeq.Domain.Common;

namespace Rafeeq.Infrastructure.Storage;

/// <summary>v1 free-path: stores uploads on local disk under {ContentRoot}/uploads (or config FileStorage:Root).</summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IHostEnvironment env, IConfiguration config)
    {
        var configured = config["FileStorage:Root"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "uploads")
            : configured;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(byte[] content, string fileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        var key = $"{Guid.NewGuid():N}{ext}";
        await File.WriteAllBytesAsync(Path.Combine(_root, key), content, ct);
        return key;
    }

    public async Task<byte[]?> ReadAsync(string storedKey, CancellationToken ct = default)
    {
        var safe = Path.GetFileName(storedKey); // guard against path traversal
        var path = Path.Combine(_root, safe);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }
}
