using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace NodeEditor.Controls;


public class Connector : TemplatedControl
{
    public static readonly StyledProperty<Point> AnchorProperty = AvaloniaProperty.Register<Connector, Point>(
        nameof(Anchor));

    public static readonly StyledProperty<object> HeaderProperty = AvaloniaProperty.Register<Connector, object>(
        nameof(Header));

    public static readonly StyledProperty<bool> IsOutputProperty = AvaloniaProperty.Register<Connector, bool>(
        nameof(IsOutput));
    
    
    public static readonly StyledProperty<bool> IsConnectedProperty = AvaloniaProperty.Register<Connector, bool>(
        nameof(IsConnected));

    public static readonly StyledProperty<IBrush> BrushProperty = AvaloniaProperty.Register<Connector, IBrush>(
        nameof(Brush));

    public IBrush Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }
    public bool IsConnected
    {
        get => GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public bool IsOutput
    {
        get => GetValue(IsOutputProperty);
        set => SetValue(IsOutputProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Point Anchor
    {
        get => GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateAnchor();
    }

    public void UpdateAnchor()
    {
        var item = this.FindAncestorOfType<CanvasItem>();
        if (item == null) return;
        var pos = new Point(Canvas.GetLeft(item), Canvas.GetTop(item));
        Point offset;
        if (IsOutput)
        {
            offset = this.TranslatePoint(new Point(Bounds.Width,Bounds.Height/2), item)!.Value;
        }
        else
        {
            offset = this.TranslatePoint(new Point(0,Bounds.Height/2), item)!.Value;
        }
        Anchor = offset + pos;
        
    }
}