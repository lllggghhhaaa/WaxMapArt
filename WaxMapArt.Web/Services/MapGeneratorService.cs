using System.IO.Compression;
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

        BlockInfo[]? blocks = null;
        Stream? exportStream = null;

        if (!request.ShouldGenerateStructure)
            return new MapGenerationResult
            {
                ProcessedImage = processedImageData,
                GeneratedImage = ditheredImageData,
                Blocks = blocks,
                ExportStream = exportStream,
                ExportMimeType = "application/octet-stream"
            };
        var generator = CreateGenerator(request.StaircaseMode);
        var output = generator.Generate(ditheredImage, processedImage, request.Palette); 
        blocks = output.Blocks;
            
        if (request.Exporter != null)
        {
            if (request.EnableRegionSplitting && request is { RegionWidthInMaps: > 0, RegionHeightInMaps: > 0 })
            {
                exportStream = ExportWithRegions(request.Exporter, request.Palette, blocks, 
                    request.WidthMultiplier, request.HeightMultiplier, request.RegionWidthInMaps, request.RegionHeightInMaps);
                
                var isLitematica = request.Exporter is LitematicaExporter;
                var fileName = isLitematica 
                    ? $"mapart.{request.Exporter.GetFileFormat()}" 
                    : $"mapart.{request.Exporter.GetFileFormat()}.zip";
                var mimeType = isLitematica 
                    ? "application/octet-stream" 
                    : "application/zip";
                
                return new MapGenerationResult
                {
                    ProcessedImage = processedImageData,
                    GeneratedImage = ditheredImageData,
                    Blocks = blocks,
                    ExportStream = exportStream,
                    ExportFileName = fileName,
                    ExportMimeType = mimeType
                };
            }
            else
            {
                exportStream = request.Exporter.SaveAsStream(request.Palette, blocks);
                return new MapGenerationResult
                {
                    ProcessedImage = processedImageData,
                    GeneratedImage = ditheredImageData,
                    Blocks = blocks,
                    ExportStream = exportStream,
                    ExportFileName = $"mapart.{request.Exporter.GetFileFormat()}",
                    ExportMimeType = "application/octet-stream"
                };
            }
        }

        return new MapGenerationResult
        {
            ProcessedImage = processedImageData,
            GeneratedImage = ditheredImageData,
            Blocks = blocks,
            ExportStream = exportStream,
            ExportMimeType = "application/octet-stream"
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

    private static Stream ExportWithRegions(IExporter exporter, Palette palette, BlockInfo[] blocks, 
        int widthMultiplier, int heightMultiplier, int regionWidthInMaps, int regionHeightInMaps)
    {
        if (blocks.Length == 0)
        {
            return exporter.SaveAsStream(palette, blocks);
        }

        var placeholderBlocks = blocks.Where(b => b.Z == 0).ToArray();
        var mapBlocks = blocks.Where(b => b.Z > 0).ToArray();
        
        if (mapBlocks.Length == 0)
        {
            return exporter.SaveAsStream(palette, blocks);
        }

        var regionWidthInBlocks = regionWidthInMaps * 128;
        var regionHeightInBlocks = regionHeightInMaps * 128;
        
        var minX = mapBlocks.Min(b => b.X);
        var maxX = mapBlocks.Max(b => b.X);
        var minZ = mapBlocks.Min(b => b.Z);
        var maxZ = mapBlocks.Max(b => b.Z);
        
        var totalWidth = maxX - minX + 1;
        var totalHeight = maxZ - minZ + 1;
        
        var regionsX = (int)Math.Ceiling((double)totalWidth / regionWidthInBlocks);
        var regionsZ = (int)Math.Ceiling((double)totalHeight / regionHeightInBlocks);
        
        if (exporter is LitematicaExporter)
        {
            return ExportLitematicaWithRegions(palette, mapBlocks, placeholderBlocks, minX, minZ, 
                regionWidthInBlocks, regionHeightInBlocks, regionsX, regionsZ);
        }
        else
        {
            return ExportVanillaWithRegions(palette, mapBlocks, placeholderBlocks, minX, minZ, 
                regionWidthInBlocks, regionHeightInBlocks, regionsX, regionsZ, exporter);
        }
    }

    private static Stream ExportLitematicaWithRegions(Palette palette, BlockInfo[] mapBlocks, BlockInfo[] placeholderBlocks, 
        int minX, int minZ, int regionWidthInBlocks, int regionHeightInBlocks, int regionsX, int regionsZ)
    {
        var exporter = new LitematicaExporter();
        return exporter.SaveAsStreamWithRegions(palette, mapBlocks, placeholderBlocks, minX, minZ, 
            regionWidthInBlocks, regionHeightInBlocks, regionsX, regionsZ);
    }

    private static Stream ExportVanillaWithRegions(Palette palette, BlockInfo[] mapBlocks, BlockInfo[] placeholderBlocks, 
        int minX, int minZ, int regionWidthInBlocks, int regionHeightInBlocks, int regionsX, int regionsZ, IExporter exporter)
    {
        var maxX = mapBlocks.Max(b => b.X);
        var maxZ = mapBlocks.Max(b => b.Z);
        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            for (var regionX = 0; regionX < regionsX; regionX++)
            {
                for (var regionZ = 0; regionZ < regionsZ; regionZ++)
                {
                    var regionMinX = minX + regionX * regionWidthInBlocks;
                    var regionMaxX = Math.Min(regionMinX + regionWidthInBlocks - 1, maxX);
                    var regionMinZ = minZ + regionZ * regionHeightInBlocks;
                    var regionMaxZ = Math.Min(regionMinZ + regionHeightInBlocks - 1, maxZ);
                    
                    var regionMapBlocks = mapBlocks
                        .Where(b => b.X >= regionMinX && b.X <= regionMaxX && 
                                   b.Z >= regionMinZ && b.Z <= regionMaxZ)
                        .ToArray();
                    
                    var regionPlaceholderBlocks = placeholderBlocks
                        .Where(b => b.X >= regionMinX && b.X <= regionMaxX)
                        .ToArray();
                    
                    if (regionMapBlocks.Length == 0 && regionPlaceholderBlocks.Length == 0) continue;
                    
                    var allRegionBlocks = regionMapBlocks.Concat(regionPlaceholderBlocks).ToArray();
                    
                    var regionMinY = allRegionBlocks.Min(b => b.Y);
                    var normalizedBlocks = allRegionBlocks
                        .Select(b => new BlockInfo(
                            b.X - regionMinX,
                            b.Y - regionMinY,
                            b.Z == 0 ? 0 : b.Z - regionMinZ + 1, 
                            b.Id,
                            b.Properties
                        ))
                        .ToArray();
                    
                    var entryName = $"region_{regionX}_{regionZ}.{exporter.GetFileFormat()}";
                    var entry = archive.CreateEntry(entryName);
                    
                    using (var entryStream = entry.Open())
                    using (var regionStream = exporter.SaveAsStream(palette, normalizedBlocks))
                    {
                        regionStream.CopyTo(entryStream);
                    }
                }
            }
        }
        
        zipStream.Seek(0, SeekOrigin.Begin);
        return zipStream;
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
    
    // Region splitting
    public bool EnableRegionSplitting { get; init; }
    public int RegionWidthInMaps { get; init; } = 1;
    public int RegionHeightInMaps { get; init; } = 1;
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
    public string? ExportFileName { get; init; }
    public string ExportMimeType { get; init; } = "application/octet-stream";
}