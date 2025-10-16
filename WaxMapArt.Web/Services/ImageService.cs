using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using WaxMapArt.Web.Database;

namespace WaxMapArt.Web.Services;

public class ImageService(IDbContextFactory<DatabaseContext> dbFactory, IConfiguration configuration, IMinioClient minioClient, ILogger<ImageService> logger)
{
    private readonly string _bucketName = configuration["MinIO:Bucket"] ??
                                          throw new InvalidOperationException("MinIO Bucket is not configured");

    public async Task<UserImage> UploadUserAvatarAsync(Guid userId, Stream imageStream, string fileName, string contentType)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        
        var user = await dbContext.Users.Include(u => u.AvatarImage).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.AvatarImage != null)
        {
            await DeleteImageInternalAsync(user.AvatarImage);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(imageStream)
            .WithObjectSize(imageStream.Length)
            .WithContentType(contentType);

        var response = await minioClient.PutObjectAsync(putObjectArgs);
        
        var image = new UserImage
        {
            FileName = fileName,
            ObjectName = response.ObjectName,
            ContentType = contentType,
            Size = imageStream.Length
        };

        dbContext.Images.Add(image);

        user.AvatarImageId = image.Id;
        await dbContext.SaveChangesAsync();

        return image;
    }
    
    public async Task<MemoryStream?> GetUserAvatarAsync(Guid userId)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();

        var user = await dbContext.Users.Include(u => u.AvatarImage).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.AvatarImage == null)
            return null;

        var memoryStream = new MemoryStream();
        try
        {
            var getArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(user.AvatarImage.ObjectName)
                .WithCallbackStream(s => s.CopyTo(memoryStream));

            await minioClient.GetObjectAsync(getArgs);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (MinioException ex)
        {
            logger.LogError(ex, "Error getting avatar for user {UserId}", userId);
            return null;
        }
    }
    
    public async Task<string?> GetUserAvatarUrlAsync(Guid? userId, int expiry = 3600)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        
        if (userId is null) return null;
        
        var user = await dbContext.Users.Include(u => u.AvatarImage).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.AvatarImage is null) return null;

        return await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(user.AvatarImage.ObjectName)
            .WithExpiry(expiry));
    }
    
    public async Task<bool> DeleteUserAvatarAsync(Guid userId)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        
        var user = await dbContext.Users.Include(u => u.AvatarImage).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.AvatarImage == null) return false;

        await DeleteImageInternalAsync(user.AvatarImage);
        user.AvatarImageId = null;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Stream> GetImageAsync(string fileName)
    {
        var memoryStream = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithCallbackStream(stream => { stream.CopyTo(memoryStream); });

        await minioClient.GetObjectAsync(getObjectArgs);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<string> GetImageUrlAsync(string fileName, int expiryInSeconds = 3600)
    {
        var presignedGetObjectArgs = new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithExpiry(expiryInSeconds);

        var url = await minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
        return url;
    }

    public async Task<bool> DeleteImageAsync(string fileName)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);

            await minioClient.RemoveObjectAsync(removeObjectArgs);
            return true;
        }
        catch (MinioException ex)
        {
            logger.LogError(ex, "Error deleting image {FileName} from MinIO", fileName);
            return false;
        }
    }

    public async Task<bool> ImageExistsAsync(string fileName)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);

            await minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch (MinioException)
        {
            return false;
        }
    }
    
    private async Task DeleteImageInternalAsync(UserImage image)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        
        try
        {
            await minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(image.ObjectName));

            dbContext.Images.Remove(image);
            await dbContext.SaveChangesAsync();
        }
        catch (MinioException ex)
        {
            logger.LogError(ex, "Error deleting image {ObjectName}", image.ObjectName);
        }
    }
}