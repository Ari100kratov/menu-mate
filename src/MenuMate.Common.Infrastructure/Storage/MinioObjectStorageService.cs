using MenuMate.Common.Application.Storage;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MenuMate.Common.Infrastructure.Storage;

/// <summary>
/// Реализация объектного хранилища поверх MinIO.
/// </summary>
public sealed class MinioObjectStorageService(
    IMinioClient minioClient,
    PublicMinioClient publicMinioClient) : IObjectStorageService
{
    /// <inheritdoc />
    public async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        try
        {
            bool exists = await minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket),
                cancellationToken);

            if (exists)
            {
                return;
            }

            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket),
                cancellationToken);
        }
        catch (InvalidBucketNameException)
        {
            throw new ObjectStorageInvalidNameException(bucket);
        }
        catch (AuthorizationException)
        {
            throw new ObjectStorageAuthorizationException();
        }
        catch (MinioException exception)
        {
            throw new ObjectStorageWriteException(exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            await minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key)
                    .WithStreamData(content)
                    .WithObjectSize(sizeBytes)
                    .WithContentType(contentType),
                cancellationToken);
        }
        catch (BucketNotFoundException)
        {
            throw new ObjectStorageBucketNotFoundException(bucket);
        }
        catch (InvalidBucketNameException)
        {
            throw new ObjectStorageInvalidNameException(bucket);
        }
        catch (InvalidObjectNameException)
        {
            throw new ObjectStorageInvalidNameException(key);
        }
        catch (AuthorizationException)
        {
            throw new ObjectStorageAuthorizationException();
        }
        catch (MinioException exception)
        {
            throw new ObjectStorageWriteException(exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            await minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key),
                cancellationToken);
        }
        catch (BucketNotFoundException)
        {
            throw new ObjectStorageBucketNotFoundException(bucket);
        }
        catch (InvalidBucketNameException)
        {
            throw new ObjectStorageInvalidNameException(bucket);
        }
        catch (InvalidObjectNameException)
        {
            throw new ObjectStorageInvalidNameException(key);
        }
        catch (AuthorizationException)
        {
            throw new ObjectStorageAuthorizationException();
        }
        catch (MinioException exception)
        {
            throw new ObjectStorageWriteException(exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<Stream> GetObjectStreamAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            var memoryStream = new MemoryStream();

            await minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)),
                cancellationToken);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (BucketNotFoundException)
        {
            throw new ObjectStorageBucketNotFoundException(bucket);
        }
        catch (ObjectNotFoundException)
        {
            throw new ObjectStorageObjectNotFoundException(bucket, key);
        }
        catch (InvalidBucketNameException)
        {
            throw new ObjectStorageInvalidNameException(bucket);
        }
        catch (InvalidObjectNameException)
        {
            throw new ObjectStorageInvalidNameException(key);
        }
        catch (AuthorizationException)
        {
            throw new ObjectStorageAuthorizationException();
        }
        catch (MinioException exception)
        {
            throw new ObjectStorageReadException(exception.Message);
        }
    }

    /// <inheritdoc />
    public Task<string> GetReadUrlAsync(string bucket, string key, TimeSpan? lifetime = null)
    {
        TimeSpan effectiveLifetime = lifetime ?? TimeSpan.FromHours(1);

        return publicMinioClient.Client.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithExpiry((int)effectiveLifetime.TotalSeconds));
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        try
        {
            await minioClient.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(key),
                cancellationToken);

            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }
}
