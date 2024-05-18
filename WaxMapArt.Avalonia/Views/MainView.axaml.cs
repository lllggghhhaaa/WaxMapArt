using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Avalonia.Controls;
using WaxMapArt.Avalonia.ViewModels;
using WaxMapArt.ImageProcessing.Dithering;
using Image = SixLabors.ImageSharp.Image;

namespace WaxMapArt.Avalonia.Views;

public partial class MainView : UserControl
{
    private Image<Rgb24>? _image;
    private Dictionary<string, Palette> _palettes = new();
    private Stopwatch _generatorWatch = new();

    public MainView()
    {
        InitializeComponent();
        CreateFiles();
        ReloadOptions();
        WatchPalettes();
        InitializePaletteEditor();
    }

    public void CreateFiles()
    {
        if (!Directory.Exists("palettes")) Directory.CreateDirectory("palettes");
    }

    public void ReloadOptions()
    {
        methodBox.Items.Clear();
        ditheringBox.Items.Clear();
        genMethodBox.Items.Clear();

        foreach (var method in Enum.GetValues<ComparisonMethod>())
            methodBox.Items.Add(method.ToString());

        foreach (var dithering in Enum.GetValues<DitheringType>())
            ditheringBox.Items.Add(dithering.ToString());

        foreach (var genMethod in Enum.GetValues<GenerateMethod>())
            genMethodBox.Items.Add(genMethod.ToString());
    }

    public void WatchPalettes()
    {
        var watcher = new FileSystemWatcher
        {
            Path = "palettes",
            Filter = "*.json",
            EnableRaisingEvents = true
        };

        watcher.Created += (sender, args) =>
        {
            var palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText(args.FullPath));
            _palettes.Add(args.Name!, palette);
            ReloadPalettes();
        };

        watcher.Deleted += (sender, args) => 
        { 
            _palettes.Remove(args.Name!);
            ReloadPalettes();
        };

        ReloadPalettes();
    }

    public void ReloadPaletteList()
    {
        _palettes.Clear();

        foreach (var file in Directory.GetFiles("palettes", "*.json"))
        {
            var palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText(file));
            _palettes.Add(Path.GetFileName(file), palette);
        }

        ReloadPaletteEditor();
    }

    public void ReloadPalettes()
    {
        ReloadPaletteList();
        paletteBox.Items.Clear();
        foreach (var palette in _palettes.Values) paletteBox.Items.Add(palette.Name);
    }

    public async void UploadClick(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image",
            AllowMultiple = false
        });

        if (files.Count <= 0) return;

        var stream = await files[0].OpenReadAsync();
        var bm = new Bitmap(stream);

        var size = new WaxSize((int)bm.Size.Width, (int)bm.Size.Height).ClampMax(256);
        bm = bm.CreateScaledBitmap(new PixelSize(size.X, size.Y));
        inputImage.Source = bm;
        stream.Seek(0, SeekOrigin.Begin);

        _image = await Image.LoadAsync<Rgb24>(stream);

        await stream.FlushAsync();
    }

    public void ReloadPalettesClick(object sender, RoutedEventArgs args) => ReloadPalettes();

    public async void PreviewClick(object sender, RoutedEventArgs args)
    {
        if (_image is null) return;

        _generatorWatch.Restart();

        var ctx = DataContext as MainViewModel;
        var palette = _palettes.ElementAt(ctx!.PaletteIndex).Value;

        var preview = new Preview(palette)
        {
            Method = ctx.Comparison,
            MapSize = ctx.MapSize,
            OutputSize = (ctx.MapSize * 128).ClampMax(384),
            Dithering = ctx.Dithering
        };

        var output = ctx.Generate switch
        {
            GenerateMethod.Staircase => preview.GeneratePreviewStaircase(_image),
            GenerateMethod.Flat => preview.GeneratePreviewFlat(_image),
            _ => preview.GeneratePreviewStaircase(_image)
        };

        var stream = new MemoryStream();
        await output.Image.SaveAsPngAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bm = new Bitmap(stream);

        var size = new WaxSize((int)bm.Size.Width, (int)bm.Size.Height).ClampMax(384);
        bm = bm.CreateScaledBitmap(new PixelSize(size.X, size.Y));
        previewImage.Source = bm;
        stream.Seek(0, SeekOrigin.Begin);

        await stream.FlushAsync();

        _generatorWatch.Stop();

        UpdateResume(ctx, output.BlockList);
    }

    public async void GenerateClick(object sender, RoutedEventArgs args)
    {
        if (_image is null) return;

        _generatorWatch.Restart();

        var ctx = DataContext as MainViewModel;
        var palette = _palettes.ElementAt(ctx!.PaletteIndex).Value;

        var generator = new Generator(palette)
        {
            Method = ctx.Comparison,
            MapSize = ctx.MapSize,
            OutputSize = (ctx.MapSize * 128).ClampMax(384),
            Dithering = ctx.Dithering
        };

        var output = ctx.Generate switch
        {
            GenerateMethod.Staircase => generator.GenerateStaircase(_image),
            GenerateMethod.Flat => generator.GenerateFlat(_image),
            _ => generator.GenerateStaircase(_image)
        };

        var stream = new MemoryStream();
        await output.Image.SaveAsPngAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bm = new Bitmap(stream);

        var size = new WaxSize((int)bm.Size.Width, (int)bm.Size.Height).ClampMax(384);
        bm = bm.CreateScaledBitmap(new PixelSize(size.X, size.Y));
        previewImage.Source = bm;
        await stream.FlushAsync();

        var nbtStream = NbtGenerator.Generate(output.Blocks);

        string? _ = await SaveFile(new FilePickerSaveOptions
        {
            Title = "Save NBT schematic",
            SuggestedFileName = "output.nbt",
            DefaultExtension = "nbt"
        }, nbtStream);

        _generatorWatch.Stop();

        UpdateResume(ctx, output.CountBlocks());
    }

    public void UpdateResume(MainViewModel ctx, Dictionary<BlockInfo, int> blockList)
    {
        var size = ctx.MapSize;

        var sb = new StringBuilder("Used Blocks: \n");
        foreach (var (info, count) in blockList)
        {
            int packs = count / 64;
            int rem = count % 64;
            double shulkers = Math.Truncate(packs / 27f * 100) / 100;

            string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
            string id = info.BlockId.Replace("minecraft:", "");

            sb.Append($"  {id}: {count} ({packCount} blocks) ({shulkers}SB)\n");
        }

        resPalette.Text = $"Palette: {_palettes.ElementAt(ctx.PaletteIndex).Value.Name}";
        resWidth.Text = $"Width: {size.X}";
        resHeight.Text = $"Height: {size.Y}";
        resMethod.Text = $"Comparison method: {Enum.GetName(ctx.Comparison)}";
        resGenMethod.Text = $"Generation method: {Enum.GetName(ctx.Generate)}";
        resDithering.Text = $"Dithering: {Enum.GetName(ctx.Dithering)}";
        resElapsed.Text = $"Elapsed time: {_generatorWatch.Elapsed:m\\:ss\\.fff}";
        resBlocks.Text = sb.ToString();
    }

    public async Task<string?> SaveFile(FilePickerSaveOptions options, Stream stream, string? startFolder = null)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        
        if (startFolder is not null) options.SuggestedStartLocation = await topLevel!.StorageProvider.TryGetFolderFromPathAsync(startFolder);
        
        var file = await topLevel!.StorageProvider.SaveFilePickerAsync(options);
        if (file is null) return null;

        using (var fs = await file.OpenWriteAsync())
        {
            await stream.CopyToAsync(fs);
            await stream.FlushAsync();
        }

        return file.Path.ToString();
    }

    // Palette Editor

    public void InitializePaletteEditor()
    {
        ReloadPaletteEditor();
    }

    public void ReloadPaletteEditor()
    {
        pePalettes.Children.Clear();

        foreach (var palette in _palettes.Values)
        {
            var eHeader = new TextBlock { Text = palette.Name };
            var eContent = new StackPanel();

            foreach (var block in palette.Colors.Values)
            {
                var pElement = new BlockInfoControl { Value = block };
                pElement.Deleted += (_, _) =>
                {
                    eContent.Children.Remove(pElement);
                    palette.Colors.Remove(block.MapId.ToString());
                };
                eContent.Children.Add(pElement);
            }

            var addButton = new Button { Content = "Add" };
            var saveButton = new Button { Content = "Save" };

            saveButton.Click += async (_, _) =>
            {
                foreach (var control in eContent.Children)
                {
                    var biControl = control as BlockInfoControl;
                    if (biControl is null) continue;

                    var block = biControl.Value;
                    palette.Colors[block.MapId.ToString()] = block;
                }

                string content = JsonConvert.SerializeObject(palette);
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

                await SaveFile(new FilePickerSaveOptions
                {
                    Title = "Save Palette",
                    SuggestedFileName = $"{palette.Name}.json",
                    DefaultExtension = "json"
                }, ms, Path.Combine(Directory.GetCurrentDirectory(), "palettes"));
            };
            addButton.Click += (_, _) =>
            {
                var pElement = new BlockInfoControl { Value = new BlockInfo { MapId = 0 } };
                pElement.Deleted += (_, _) =>
                {
                    eContent.Children.Remove(pElement);
                    palette.Colors.Remove(pElement.Value.MapId.ToString());
                };
                
                eContent.Children.Insert(0, pElement);
            };

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(addButton);
            buttons.Children.Add(saveButton);
                        
            eContent.Children.Add(buttons);

            pePalettes.Children.Add(new Expander
            {
                Name = palette.Name,
                ExpandDirection = ExpandDirection.Down,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 1),
                Header = eHeader,
                Content = eContent
            });
        }
    }
}