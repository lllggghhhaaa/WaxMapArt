using SkiaSharp;

namespace WaxMapArt.Processing;

public enum ResizeMethod
{
    Stretch,
    Pad,
    Crop,
    Min,
    Max
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

    public float BlurRadius { get; set; }
}

public class ImageProcessor
{
    public ProcessingOptions Options { get; set; } = new();

    public SKBitmap Process(SKBitmap inputImage)
    {
        using var adjusted = ApplyColorAdjustments(inputImage);
        return ResizeImage(adjusted, Options.TargetWidth, Options.TargetHeight);
    }

    private SKBitmap ApplyColorAdjustments(SKBitmap source)
    {
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height));
        var canvas = surface.Canvas;
        using var paint = new SKPaint { IsAntialias = true };

        SKColorFilter? colorFilter = null;
        SKImageFilter? imageFilter = null;

        if (Math.Abs(Options.Saturation - 1.0f) > float.Epsilon)
        {
            var s = Options.Saturation;
            const float rw = 0.2126f;
            const float gw = 0.7152f;
            const float bw = 0.0722f;

            colorFilter = SKColorFilter.CreateColorMatrix(new[]
            {
                rw * (1 - s) + s, gw * (1 - s), bw * (1 - s), 0, 0,
                rw * (1 - s), gw * (1 - s) + s, bw * (1 - s), 0, 0,
                rw * (1 - s), gw * (1 - s), bw * (1 - s) + s, 0, 0,
                0, 0, 0, 1, 0
            });
        }

        if (Math.Abs(Options.Brightness - 1.0f) > float.Epsilon || Math.Abs(Options.Contrast - 1.0f) > float.Epsilon)
        {
            var c = Options.Contrast;
            var b = Options.Brightness;
            var offset = (b - 1.0f) * c;

            var cf = SKColorFilter.CreateColorMatrix(new[]
            {
                c, 0, 0, 0, offset,
                0, c, 0, 0, offset,
                0, 0, c, 0, offset,
                0, 0, 0, 1, 0
            });

            colorFilter = colorFilter is null ? cf : SKColorFilter.CreateCompose(cf, colorFilter);
        }

        if (Options.BlurRadius > 0)
            imageFilter = SKImageFilter.CreateBlur(Options.BlurRadius, Options.BlurRadius);

        canvas.Clear(SKColors.Transparent);

        if (colorFilter is not null) paint.ColorFilter = colorFilter;
        if (imageFilter is not null) paint.ImageFilter = imageFilter;

        canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions(), paint);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        var result = new SKBitmap(source.Width, source.Height);
        snapshot.ReadPixels(result.Info, result.GetPixels(), result.Info.RowBytes, 0, 0);
        return result;
    }

    private SKBitmap ResizeImage(SKBitmap source, int targetWidth, int targetHeight)
    {
        using var surface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight));
        var canvas = surface.Canvas;
        using var paint = new SKPaint { IsAntialias = true };

        var bg = Options.ResizeMethod == ResizeMethod.Pad ? ParseHexColor(Options.PadColor) : SKColors.Transparent;
        canvas.Clear(bg);

        using var image = SKImage.FromBitmap(source);
        var srcRect = Options.ResizeMethod == ResizeMethod.Crop
            ? CalculateCropRect(source.Width, source.Height, targetWidth, targetHeight)
            : new SKRect(0, 0, source.Width, source.Height);

        var dstRect = CalculateDestinationRect(source.Width, source.Height, targetWidth, targetHeight);
        canvas.DrawImage(image, srcRect, dstRect, new SKSamplingOptions(), paint);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        var result = new SKBitmap(targetWidth, targetHeight);
        snapshot.ReadPixels(result.Info, result.GetPixels(), result.Info.RowBytes, 0, 0);
        return result;
    }

    private SKRect CalculateDestinationRect(int srcW, int srcH, int dstW, int dstH)
    {
        return Options.ResizeMethod switch
        {
            ResizeMethod.Stretch => new SKRect(0, 0, dstW, dstH),
            ResizeMethod.Crop => new SKRect(0, 0, dstW, dstH),
            ResizeMethod.Pad or ResizeMethod.Min => FitRect(srcW, srcH, dstW, dstH, fitInside: true),
            ResizeMethod.Max => FitRect(srcW, srcH, dstW, dstH, fitInside: false),
            _ => new SKRect(0, 0, dstW, dstH)
        };
    }

    private SKRect CalculateCropRect(int srcW, int srcH, int dstW, int dstH)
    {
        var srcAspect = (float)srcW / srcH;
        var dstAspect = (float)dstW / dstH;

        int cropW, cropH, cropX, cropY;

        if (srcAspect > dstAspect)
        {
            cropH = srcH;
            cropW = (int)(srcH * dstAspect);
            cropX = (srcW - cropW) / 2 + Options.CropOffsetX;
            cropY = Options.CropOffsetY;
        }
        else
        {
            cropW = srcW;
            cropH = (int)(srcW / dstAspect);
            cropX = Options.CropOffsetX;
            cropY = (srcH - cropH) / 2 + Options.CropOffsetY;
        }

        cropX = Math.Max(0, Math.Min(cropX, srcW - cropW));
        cropY = Math.Max(0, Math.Min(cropY, srcH - cropH));

        return new SKRect(cropX, cropY, cropX + cropW, cropY + cropH);
    }

    private static SKRect FitRect(int srcW, int srcH, int dstW, int dstH, bool fitInside)
    {
        var ratio = fitInside
            ? Math.Min((float)dstW / srcW, (float)dstH / srcH)
            : Math.Max((float)dstW / srcW, (float)dstH / srcH);

        var newW = srcW * ratio;
        var newH = srcH * ratio;
        var dx = (dstW - newW) / 2f;
        var dy = (dstH - newH) / 2f;

        return new SKRect(dx, dy, dx + newW, dy + newH);
    }

    private static SKColor ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return SKColors.White;
        if (hex[0] == '#') hex = hex[1..];

        try
        {
            return hex.Length switch
            {
                6 => new SKColor(
                    Convert.ToByte(hex[..2], 16),
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16)),
                8 => new SKColor(
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16),
                    Convert.ToByte(hex[6..8], 16),
                    Convert.ToByte(hex[..2], 16)),
                _ => SKColors.White
            };
        }
        catch
        {
            return SKColors.White;
        }
    }
}
