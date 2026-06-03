namespace MenuMate.Common.Infrastructure.Storage;

/// <summary>
/// Настройки подключения к MinIO.
/// </summary>
public sealed class MinioOptions
{
    /// <summary>
    /// Адрес MinIO API.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Ключ доступа.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Секретный ключ.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Бакет по умолчанию для изображений.
    /// </summary>
    public string ImagesBucketName { get; set; } = "images";

    /// <summary>
    /// Использовать TLS при подключении.
    /// </summary>
    public bool UseSsl { get; set; }
}
