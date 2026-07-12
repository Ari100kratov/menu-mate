using Minio;

namespace MenuMate.Common.Infrastructure.Storage;

/// <summary>
/// Клиент MinIO с публичным адресом, предназначенный для создания подписанных ссылок.
/// </summary>
public sealed class PublicMinioClient(IMinioClient client) : IDisposable
{
    /// <summary>
    /// Клиент, настроенный на публичный адрес хранилища.
    /// </summary>
    public IMinioClient Client { get; } = client;

    /// <inheritdoc />
    public void Dispose() => Client.Dispose();
}
