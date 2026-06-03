namespace MenuMate.Common.Application.Storage;

/// <summary>
/// Абстракция S3-совместимого хранилища объектов.
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Создает бакет, если он еще не существует.
    /// </summary>
    Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет объект в S3-совместимом хранилище.
    /// </summary>
    Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет объект из S3-совместимого хранилища.
    /// </summary>
    Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает поток объекта по бакету и ключу.
    /// </summary>
    Task<Stream> GetObjectStreamAsync(string bucket, string key, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает временную ссылку для прямого чтения объекта клиентом.
    /// </summary>
    Task<string> GetReadUrlAsync(string bucket, string key, TimeSpan? lifetime = null);

    /// <summary>
    /// Проверяет наличие объекта в бакете.
    /// </summary>
    Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken);
}
