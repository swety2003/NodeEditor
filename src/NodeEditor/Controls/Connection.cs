using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NodeEditor.Controls;

public class Connection : Control
{
    public Connection()
    {
        AffectsRender<Connection>(SourceProperty, TargetProperty);
        AffectsRender<Connection>(BrushProperty, Brush1Property);
    }


    public static readonly StyledProperty<Point> SourceProperty = AvaloniaProperty.Register<Connection, Point>(
        nameof(Source));

    public Point Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<Point> TargetProperty = AvaloniaProperty.Register<Connection, Point>(
        nameof(Target));

    public Point Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly StyledProperty<IBrush> BrushProperty = AvaloniaProperty.Register<Connection, IBrush>(
        nameof(Brush), Brushes.Red);

    public IBrush Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> Brush1Property = AvaloniaProperty.Register<Connection, IBrush>(
        nameof(Brush1));

    public IBrush Brush1
    {
        get => GetValue(Brush1Property);
        set => SetValue(Brush1Property, value);
    }

    public override void Render(DrawingContext context)
    {
        ConnectNodesWithCurve(context, Source, Target, 6, Brush1);
        ConnectNodesWithCurve(context, Source, Target, 2, Brush);
        base.Render(context);
    }

    /// <summary>
    /// 带简单弯曲的连接
    /// </summary>
    public static void ConnectNodesWithCurve(DrawingContext context,
        Point source,
        Point target, double thickness = 2,
        IBrush? brush = null)
    {

        // 自动计算中点弯曲
        Point mid = new Point((source.X + target.X) / 2, (source.Y + target.Y) / 2);

        // 稍微弯曲的控制点
        Point cp1 = new Point(source.X + (target.X - source.X) * 0.3, source.Y);
        Point cp2 = new Point(target.X - (target.X - source.X) * 0.3, target.Y);

        // 绘制曲线
        var geometry = new PathGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(source, false);
            ctx.CubicBezierTo(cp1, cp2, target);
            ctx.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(brush, thickness), geometry);
    }

    #region Obsolete

    private Rect CalculateRect(Point p1, Point p2)
    {
        double left = Math.Min(p1.X, p2.X);
        double top = Math.Min(p1.Y, p2.Y);
        double right = Math.Max(p1.X, p2.X);
        double bottom = Math.Max(p1.Y, p2.Y);

        return new Rect(left, top, right - left, bottom - top);
    }

    private (Point relativeTarget, Point relativeSource)
        CalculateRelativePoints(Point target, Point source, Rect rect)
    {
        Point relativeTarget = new Point(target.X - rect.X, target.Y - rect.Y);
        Point relativeSource = new Point(source.X - rect.X, source.Y - rect.Y);

        return (relativeTarget, relativeSource);
    }

    #endregion
}


public class PendingConnection: Connection
{
    private string _tip = "This is tip";

    public static readonly DirectProperty<PendingConnection, string> TipProperty = AvaloniaProperty.RegisterDirect<PendingConnection, string>(
        nameof(Tip), o => o.Tip, (o, v) => o.Tip = v);

    public string Tip
    {
        get => _tip;
        set => SetAndRaise(TipProperty, ref _tip, value);
    }
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawText(new FormattedText(Tip?? "", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,Typeface.Default, 18d,Brush), Target - new Vector(0,20));
    }
}