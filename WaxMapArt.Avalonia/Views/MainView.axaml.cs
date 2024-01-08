using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Collections.Generic;
using WaxMapArt.Avalonia.ViewModels;
using WaxMapArt.ImageProcessing.Dithering;
using System.Linq;

namespace WaxMapArt.Avalonia.Views;

public partial class MainView : UserControl
{
    private Image<Rgb24>? _image = null;
    private Dictionary<string, Palette> _palettes = new();

    public MainView()
    {
        InitializeComponent();
        ReloadOptions();
        WatchPalettes();
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
            Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText(args.FullPath));
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
        if (!Directory.Exists("palettes")) Directory.CreateDirectory("palettes");

        _palettes.Clear();

        foreach (var file in Directory.GetFiles("palettes", "*.json"))
        {
            Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText(file));
            _palettes.Add(Path.GetFileName(file), palette);
        }
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

        Stream stream = await files[0].OpenReadAsync();

        inputImage.Source = new Bitmap(stream);
        stream.Seek(0, SeekOrigin.Begin);
        _image = await SixLabors.ImageSharp.Image.LoadAsync<Rgb24>(stream);

        await stream.FlushAsync();
    }

    public void ReloadPalettesClick(object sender, RoutedEventArgs args) => ReloadPalettes();

    public async void PreviewClick(object sender, RoutedEventArgs args)
    {
        if (_image is null) return;

        var ctx = DataContext as MainViewModel;

        Palette palette = _palettes.ElementAt(ctx!.PaletteIndex).Value;

        var preview = new Preview(palette)
        {
            Method = ctx.Comparison,
            MapSize = ctx.MapSize,
            OutputSize = new WaxSize(512, 512),
            Dithering = ctx.Dithering
        };

        PreviewOutput previewOutput = ctx.Generate switch
        {
            GenerateMethod.Staircase => preview.GeneratePreviewStaircase(_image),
            GenerateMethod.Flat => preview.GeneratePreviewFlat(_image),
            _ => preview.GeneratePreviewStaircase(_image)
        };

        Stream stream = new MemoryStream();
        await previewOutput.Image.SaveAsPngAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        previewImage.Source = new Bitmap(stream);
        await stream.FlushAsync();

        UpdateResume(ctx);
    }

    public void UpdateResume(MainViewModel ctx)
    {
        var size = ctx.MapSize;

        // resPalette.Text = $"Palette: {palette.Name}";
        resWidth.Text = $"Width: {size.X}";
        resHeight.Text = $"Height: {size.Y}";
        resMethod.Text = $"Comparison method: {Enum.GetName(ctx.Comparison)}";
        resGenMethod.Text = $"Generation method: {Enum.GetName(ctx.Generate)}";
        resDithering.Text = $"Dithering: {Enum.GetName(ctx.Dithering)}";
    }
}
