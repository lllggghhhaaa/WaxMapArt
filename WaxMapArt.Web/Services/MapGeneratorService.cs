using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Dithering;
using WaxMapArt.Entities;
using WaxMapArt.Exporter;
using WaxMapArt.Generator;
using WaxMapArt.Processing;

namespace WaxMapArt.Web.Services;

public class MapGeneratorService(ILogger<MapGeneratorService> logger)
{
    public MapGenerationResult Generate(MapGenerationRequest request)
    {
        // Step 1: Process image (resize, color adjustments)
        var processor = new ImageProcessor
        {
            Options = new ProcessingOptions
            {
                TargetWidth = request.WidthMultiplier * 128,
                TargetHeight = request.HeightMultiplier * 128,
                ResizeMethod = request.ResizeMethod,
                Saturation = request.Saturation,
                Brightness = request.Brightness,
                Contrast = request.Contrast,
                CropOffsetX = request.CropOffsetX,
                CropOffsetY = request.CropOffsetY,
                PadColor = request.PadColor,
                BlurRadius = request.BlurRadius
            }
        };

        using var processedImage = processor.Process(request.InputImage);
        var processedImageData = BitmapToBytes(processedImage);

        // Step 2: Apply dithering
        var dithering = CreateDithering(request.DitheringMode, request.DitheringOptions);
        var colorComparison = CreateColorComparison(request.ComparisonMode);
        
        var ditheredImage = dithering.ApplyDithering(
            processedImage, 
            request.Palette, 
            colorComparison, 
            request.StaircaseMode,
            request.AdaptiveStaircaseThreshold
        );
        
        var ditheredImageData = BitmapToBytes(ditheredImage);

        // Step 3: Generate structure and export if requested
        BlockInfo[]? blocks = null;
        Stream? exportStream = null;

        if (!request.ShouldGenerateStructure)
            return new MapGenerationResult
            {
                ProcessedImage = processedImageData,
                GeneratedImage = ditheredImageData,
                Blocks = blocks,
                ExportStream = exportStream
            };
        var generator = CreateGenerator(request.StaircaseMode);
        var output = generator.Generate(ditheredImage, processedImage, request.Palette); 
        blocks = output.Blocks;
            
        if (request.Exporter != null)
        {
            exportStream = request.Exporter.SaveAsStream(request.Palette, blocks);
        }

        return new MapGenerationResult
        {
            ProcessedImage = processedImageData,
            GeneratedImage = ditheredImageData,
            Blocks = blocks,
            ExportStream = exportStream
        };
    }

    private static IDithering CreateDithering(DitheringMode mode, DitheringOptions options)
    {
        return mode switch
        {
            DitheringMode.None => new NoneDithering(),
            DitheringMode.Atkinson => new AtkinsonDithering(
                options.ErrorDiffusionStrength, 
                options.SerpentineScanning
            ),
            DitheringMode.FloydSteinberg => new FloydSteinbergDithering(
                options.ErrorDiffusionStrength, 
                options.SerpentineScanning
            ),
            DitheringMode.JarvisJudiceNinke => new JarvisJudiceNinkeDithering(
                options.ErrorDiffusionStrength, 
                options.SerpentineScanning
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static IColorComparison CreateColorComparison(ComparisonMode mode)
    {
        return mode switch
        {
            ComparisonMode.Rgb => new RgbColorComparison(),
            ComparisonMode.Cie76 => new Cie76ColorComparison(),
            ComparisonMode.Cie94 => new Cie94ColorComparison(),
            ComparisonMode.CieDe2000 => new CieDe2000ColorComparison(),
            ComparisonMode.Cmc => new CmcColorComparison(),
            ComparisonMode.DeltaEITP => new DeltaEITPColorComparison(),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static IGenerator CreateGenerator(StaircaseMode type)    
    {
        return type switch
        {
            StaircaseMode.Flat => new FlatGenerator(),
            StaircaseMode.Staircase or StaircaseMode.AdaptiveStaircase => new StaircaseGenerator(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static byte[] BitmapToBytes(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}

public class MapGenerationRequest
{
    public required SKBitmap InputImage { get; init; }
    public required Palette Palette { get; init; }
    
    // Size options
    public int WidthMultiplier { get; init; } = 1;
    public int HeightMultiplier { get; init; } = 1;
    
    // Resize options
    public ResizeMethod ResizeMethod { get; init; } = ResizeMethod.Crop;
    public int CropOffsetX { get; init; }
    public int CropOffsetY { get; init; }
    public string PadColor { get; init; } = "#FFFFFF";
    
    // Color adjustments
    public float Saturation { get; init; } = 1.0f;
    public float Brightness { get; init; } = 1.0f;
    public float Contrast { get; init; } = 1.0f;
    public float BlurRadius { get; init; } = 1.0f;
    
    // Dithering
    public DitheringMode DitheringMode { get; init; }
    public DitheringOptions DitheringOptions { get; init; } = new();
    
    // Comparison
    public ComparisonMode ComparisonMode { get; init; }
    
    // Generation
    public StaircaseMode StaircaseMode { get; init; }
    public bool ShouldGenerateStructure { get; init; }
    public double AdaptiveStaircaseThreshold { get; init; }
    public IExporter? Exporter { get; init; }
}

public class DitheringOptions
{
    public float ErrorDiffusionStrength { get; init; } = 1.0f;
    public bool SerpentineScanning { get; init; }
}

public class MapGenerationResult
{
    public required byte[] ProcessedImage { get; init; }
    public required byte[] GeneratedImage { get; init; }
    public BlockInfo[]? Blocks { get; init; }
    public Stream? ExportStream { get; init; }
}