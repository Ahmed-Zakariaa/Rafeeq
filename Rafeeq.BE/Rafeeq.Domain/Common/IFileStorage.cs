namespace Rafeeq.Domain.Common;

/// <summary>
/// Abstraction over file storage. v1 uses local disk (no cloud) — see LocalFileStorage.
/// Returns an opaque storage key; the original filename/content-type live in the DB.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(byte[] content, string fileName, CancellationToken ct = default);
    Task<byte[]?> ReadAsync(string storedKey, CancellationToken ct = default);
}
