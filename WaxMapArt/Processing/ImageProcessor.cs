using SkiaSharp;

namespace WaxMapArt.Processing;

public class ImageProcessor
{
    public ProcessingOptions Options { get; set; } = new();

    public SKBitmap Process(SKBitmap inputImage)
    {
        using var adjustedImage = ApplyColorAdjustments(inputImage);
        
        var resized = ResizeImage(adjustedImage, Options.TargetWidth, Options.TargetHeight);
        
        return resized;
    }

    private SKBitmap ApplyColorAdjustments(SKBitmap source)
    {
        using var surface =
            SKSurface.Create(new SKImageInfo(source.Width, source.Height, source.ColorType, source.AlphaType));
        var canvas = surface.Canvas;
        using var paint = new SKPaint { IsAntialias = true };

        SKColorFilter? colorFilter = null;
        SKImageFilter? imageFilter = null;

        // Saturation
        if (Math.Abs(Options.Saturation - 1.0f) > float.Epsilon)
        {
            var s = Options.Saturation;
            const float rw = 0.2126f;
            const float gw = 0.7152f;
            const float bw = 0.0722f;

            var mSat = new[]
            {
                rw * (1 - s) + s, gw * (1 - s), bw * (1 - s), 0, 0,
                rw * (1 - s), gw * (1 - s) + s, bw * (1 - s), 0, 0,
                rw * (1 - s), gw * (1 - s), bw * (1 - s) + s, 0, 0,
                0, 0, 0, 1, 0
            };

            colorFilter = SKColorFilter.CreateColorMatrix(mSat);
        }

        // Brightness and Contrast
        if (Math.Abs(Options.Brightness - 1.0f) > float.Epsilon || Math.Abs(Options.Contrast - 1.0f) > float.Epsilon)
        {
            var brightness = Options.Brightness;
            var contrast = Options.Contrast;
            var bOffset = (brightness - 1.0f) * contrast;

            var mCombined = new[]
            {
                contrast, 0, 0, 0, bOffset,
                0, contrast, 0, 0, bOffset,
                0, 0, contrast, 0, bOffset,
                0, 0, 0, 1, 0
            };

            var cfCombined = SKColorFilter.CreateColorMatrix(mCombined);
            colorFilter = colorFilter is null ? cfCombined : SKColorFilter.CreateCompose(cfCombined, colorFilter);
        }

        // 👇 Blur
        if (Options.BlurRadius > 0)
        {
            imageFilter = SKImageFilter.CreateBlur(Options.BlurRadius, Options.BlurRadius);
        }

        canvas.Clear(SKColors.Transparent);

        if (colorFilter is not null)
            paint.ColorFilter = colorFilter;

        if (imageFilter is not null)
            paint.ImageFilter = imageFilter;

        canvas.DrawBitmap(source, 0, 0, paint);
        canvas.Flush();

        using var finalImg = surface.Snapshot();
        var result = new SKBitmap(source.Width, source.Height);
        finalImg.ReadPixels(result.Info, result.GetPixels(), result.Info.RowBytes, 0, 0);

        return result;
    }

    private SKBitmap ResizeImage(SKBitmap source, int targetWidth, int targetHeight)
    {
        using var resizeSurface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight, source.ColorType, source.AlphaType));
        var resizeCanvas = resizeSurface.Canvas;
        using var paint = new SKPaint { FilterQuality = SKFilterQuality.High, IsAntialias = true };

        var bg = Options.ResizeMethod == ResizeMethod.Pad ? ParseHexColor(Options.PadColor) : SKColors.Transparent;
        resizeCanvas.Clear(bg);

        using var workingImg = SKImage.FromBitmap(source);

        var srcRect = Options.ResizeMethod == ResizeMethod.Crop ? CalculateCropRect(workingImg, targetWidth, targetHeight) : new SKRect(0, 0, workingImg.Width, workingImg.Height);

        var dstRect = CalculateDestinationRect(workingImg, targetWidth, targetHeight);

        resizeCanvas.DrawImage(workingImg, srcRect, dstRect, paint);
        resizeCanvas.Flush();

        using var finalImg = resizeSurface.Snapshot();
        var result = new SKBitmap(targetWidth, targetHeight);
        finalImg.ReadPixels(result.Info, result.GetPixels(), result.Info.RowBytes, 0, 0);

        return result;
    }
    
    private SKRect CalculateDestinationRect(SKImage source, int targetWidth, int targetHeight)
    {
        return Options.ResizeMethod switch
        {
            ResizeMethod.Stretch => new SKRect(0, 0, targetWidth, targetHeight),
            ResizeMethod.Crop    => new SKRect(0, 0, targetWidth, targetHeight),
            ResizeMethod.Pad or ResizeMethod.Min => CalculateFitRect(source, targetWidth, targetHeight, true),
            ResizeMethod.Max => CalculateFitRect(source, targetWidth, targetHeight, false),
            _ => new SKRect(0, 0, targetWidth, targetHeight)
        };
    }

    private SKRect CalculateCropRect(SKImage source, int targetWidth, int targetHeight)
    {
        var sourceAspect = (float)source.Width / source.Height;
        var targetAspect = (float)targetWidth / targetHeight;

        int cropWidth, cropHeight, cropX, cropY;

        if (sourceAspect > targetAspect)
        {
            cropHeight = source.Height;
            cropWidth = (int)(source.Height * targetAspect);
            cropX = (source.Width - cropWidth) / 2 + Options.CropOffsetX;
            cropY = Options.CropOffsetY;
        }
        else
        {
            cropWidth = source.Width;
            cropHeight = (int)(source.Width / targetAspect);
            cropX = Options.CropOffsetX;
            cropY = (source.Height - cropHeight) / 2 + Options.CropOffsetY;
        }

        cropX = Math.Max(0, Math.Min(cropX, source.Width - cropWidth));
        cropY = Math.Max(0, Math.Min(cropY, source.Height - cropHeight));

        return new SKRect(cropX, cropY, cropX + cropWidth, cropY + cropHeight);
    }
    
    private SKRect CalculateFitRect(SKImage source, int targetWidth, int targetHeight, bool fitInside)
    {
        var ratio = fitInside 
            ? Math.Min((float)targetWidth / source.Width, (float)targetHeight / source.Height)
            : Math.Max((float)targetWidth / source.Width, (float)targetHeight / source.Height);
            
        var newW = source.Width * ratio;
        var newH = source.Height * ratio;
        var dx = (targetWidth - newW) / 2f;
        var dy = (targetHeight - newH) / 2f;
        
        return new SKRect(dx, dy, dx + newW, dy + newH);
    }

    private static SKColor ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return SKColors.White;
        if (hex.StartsWith('#')) hex = hex[1..];
        
        try
        {
            switch (hex.Length)
            {
                case 6:
                {
                    var r = Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return new SKColor(r, g, b);
                }
                case 8:
                {
                    var a = Convert.ToByte(hex.Substring(0, 2), 16);
                    var r = Convert.ToByte(hex.Substring(2, 2), 16);
                    var g = Convert.ToByte(hex.Substring(4, 2), 16);
                    var b = Convert.ToByte(hex.Substring(6, 2), 16);
                    return new SKColor(r, g, b, a);
                }
            }
        }
        catch
        {
            // ignore and fallback
        }

        return SKColors.White;
    }
}

public class ProcessingOptions
{
    public int TargetWidth { get; set; } = 128;
    public int TargetHeight { get; set; } = 128;
    public ResizeMethod ResizeMethod { get; set; } = ResizeMethod.Crop;
    
    public float Saturation { get; set; } = 1.0f;
    public float Brightness { get; set; } = 1.0f;
    public float Contrast { get; set; } = 1.0f;
    
    public int CropOffsetX { get; set; }
    public int CropOffsetY { get; set; }
    public string PadColor { get; set; } = "#FFFFFF";
    
    public float BlurRadius { get; set; } = 0f;
}

public enum ResizeMethod
{
    Stretch,
    Pad,
    Crop,
    Min,
    Max
}