using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace WaxMapArt.Avalonia.Controls;

public class BlockInfoControl : TemplatedControl
{
    public static readonly StyledProperty<BlockInfo> ValueProperty =
        AvaloniaProperty.Register<BlockInfoControl, BlockInfo>(
            nameof(Value),
            defaultValue: new BlockInfo { BlockId = "Null" });
    
    public static readonly RoutedEvent<RoutedEventArgs> DeletedEvent =
        RoutedEvent.Register<BlockInfoControl, RoutedEventArgs>("Deleted", RoutingStrategies.Direct);

    public event EventHandler<RoutedEventArgs> Deleted
    {
        add => AddHandler(DeletedEvent, value);
        remove => RemoveHandler(DeletedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var button = e.NameScope.Find<Button>("DeleteButton");
        button!.Click += (_, _) => RaiseEvent(new RoutedEventArgs(DeletedEvent));
    }

    public BlockInfo Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int ValueMapId
    {
        get => Value.MapId;
        set
        {
            var block = Value;
            block.MapId = value;
            Value = block;
        }
    }

    public string ValueId
    {
        get => Value.BlockId;
        set
        {
            var block = Value;
            block.BlockId = value;
            Value = block;
        }
    }

    public Color ValueColor
    {
        get => new(255, Value.Color.R, Value.Color.G, Value.Color.B);
        set
        {
            var block = Value;
            block.Color = new WaxColor(value.R, value.G, value.B);
            Value = block;
        }
    }
}
