using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using WaxMapArt.Avalonia.ViewModels;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt.Avalonia.Views;

public partial class MainView : UserControl
{

    private Image<Rgb24>? _image = null;

    public MainView()
    {
        InitializeComponent();
        ReloadOptions();
        ReloadPalettes();
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

    public async void ReloadPalettes()
    {
        var samplePalette = JsonConvert.DeserializeObject<Palette>(await File.ReadAllTextAsync("palette.json"));
        paletteBox.Items.Clear();

        paletteBox.Items.Add(samplePalette.Name);
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

    public async void PreviewClick(object sender, RoutedEventArgs args)
    {
        if (_image is null) return;

        var ctx = DataContext as MainViewModel;

        Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText("palette.json"));

        var preview = new Preview(palette)
        {
            Method = ctx!.Comparison,
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
