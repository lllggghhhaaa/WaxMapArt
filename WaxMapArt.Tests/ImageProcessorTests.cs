using SkiaSharp;
using WaxMapArt.Processing;

namespace WaxMapArt.Tests;

public class ImageProcessorTests
{
    private static SKBitmap CreateTestBitmap(int width, int height)
    {
        var bmp = new SKBitmap(width, height);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            bmp.SetPixel(x, y, new SKColor((byte)(x * 255 / width), (byte)(y * 255 / height), 128));
        return bmp;
    }

    [Fact]
    public void Process_DefaultOptions_ReturnsCorrectSize()
    {
        var image = CreateTestBitmap(256, 256);
        var processor = new ImageProcessor();
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
        Assert.Equal(128, result.Height);
    }

    [Fact]
    public void Process_CustomSize_ReturnsCorrectSize()
    {
        var image = CreateTestBitmap(200, 200);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions { TargetWidth = 64, TargetHeight = 64 }
        };
        var result = processor.Process(image);
        Assert.Equal(64, result.Width);
        Assert.Equal(64, result.Height);
    }

    [Fact]
    public void Process_StretchResize_FillsTarget()
    {
        var image = CreateTestBitmap(100, 200);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 50,
                TargetHeight = 50,
                ResizeMethod = ResizeMethod.Stretch
            }
        };
        var result = processor.Process(image);
        Assert.Equal(50, result.Width);
        Assert.Equal(50, result.Height);
    }

    [Fact]
    public void Process_PadResize_DoesNotThrow()
    {
        var image = CreateTestBitmap(100, 200);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 128,
                TargetHeight = 128,
                ResizeMethod = ResizeMethod.Pad
            }
        };
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
        Assert.Equal(128, result.Height);
    }

    [Fact]
    public void Process_MinResize_DoesNotThrow()
    {
        var image = CreateTestBitmap(100, 200);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 128,
                TargetHeight = 128,
                ResizeMethod = ResizeMethod.Min
            }
        };
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
        Assert.Equal(128, result.Height);
    }

    [Fact]
    public void Process_MaxResize_DoesNotThrow()
    {
        var image = CreateTestBitmap(100, 200);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 128,
                TargetHeight = 128,
                ResizeMethod = ResizeMethod.Max
            }
        };
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
        Assert.Equal(128, result.Height);
    }

    [Fact]
    public void Process_Saturation_DoesNotThrow()
    {
        var image = CreateTestBitmap(16, 16);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 16,
                TargetHeight = 16,
                Saturation = 0.5f,
                ResizeMethod = ResizeMethod.Stretch
            }
        };
        var result = processor.Process(image);
        Assert.Equal(16, result.Width);
    }

    [Fact]
    public void Process_BrightnessAndContrast_DoesNotThrow()
    {
        var image = CreateTestBitmap(16, 16);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 16,
                TargetHeight = 16,
                Brightness = 1.2f,
                Contrast = 1.1f,
                ResizeMethod = ResizeMethod.Stretch
            }
        };
        var result = processor.Process(image);
        Assert.Equal(16, result.Width);
    }

    [Fact]
    public void Process_Blur_DoesNotThrow()
    {
        var image = CreateTestBitmap(16, 16);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 16,
                TargetHeight = 16,
                BlurRadius = 2.0f,
                ResizeMethod = ResizeMethod.Stretch
            }
        };
        var result = processor.Process(image);
        Assert.Equal(16, result.Width);
    }

    [Fact]
    public void Process_CombinedAdjustments_DoesNotThrow()
    {
        var image = CreateTestBitmap(32, 32);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 64,
                TargetHeight = 64,
                Saturation = 1.5f,
                Brightness = 0.8f,
                Contrast = 1.3f,
                BlurRadius = 1.0f,
                ResizeMethod = ResizeMethod.Pad,
                PadColor = "#FF0000"
            }
        };
        var result = processor.Process(image);
        Assert.Equal(64, result.Width);
        Assert.Equal(64, result.Height);
    }

    [Fact]
    public void Process_PadWithColor_DoesNotThrow()
    {
        var image = CreateTestBitmap(50, 100);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 128,
                TargetHeight = 128,
                ResizeMethod = ResizeMethod.Pad,
                PadColor = "#00FF00"
            }
        };
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
    }

    [Fact]
    public void Process_CropWithOffset_DoesNotThrow()
    {
        var image = CreateTestBitmap(200, 100);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 64,
                TargetHeight = 64,
                ResizeMethod = ResizeMethod.Crop,
                CropOffsetX = 10,
                CropOffsetY = 5
            }
        };
        var result = processor.Process(image);
        Assert.Equal(64, result.Width);
        Assert.Equal(64, result.Height);
    }

    [Fact]
    public void Process_OneByOneImage_DoesNotThrow()
    {
        var image = new SKBitmap(1, 1);
        image.SetPixel(0, 0, SKColors.Red);
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = 128,
                TargetHeight = 128,
                ResizeMethod = ResizeMethod.Stretch
            }
        };
        var result = processor.Process(image);
        Assert.Equal(128, result.Width);
    }
}
