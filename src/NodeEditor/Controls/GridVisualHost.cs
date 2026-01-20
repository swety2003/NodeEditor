using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace NodeEditor.Controls;

/// <summary>
/// see https://github.com/DynamoDS/Dynamo/blob/master/src/DynamoCoreWpf/Controls/InfiniteGridView.xaml.cs
/// </summary>
public class GridVisualHost : Control
{
    public static readonly StyledProperty<ITransform> ViewportTransformProperty = AvaloniaProperty.Register<GridVisualHost, ITransform>(
        nameof(ViewportTransform));

    public ITransform ViewportTransform
    {
        get => GetValue(ViewportTransformProperty);
        set => SetValue(ViewportTransformProperty, value);
    }
    
    #region Private Class Data Members

    private const int MinorDivisions = 5;
    private const int MajorGridLineSpacing = 100;
    private const double MinMajorGridSpacing = 50;
    private const double MaxMajorGridSpacing = MinMajorGridSpacing * MinorDivisions;
    private const double ScaleFactor = MaxMajorGridSpacing / MinMajorGridSpacing;

    private Pen majorGridPen, minorGridPen;

    #endregion

    #region Public Class Methods

    public GridVisualHost()
    {
        var majorBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
        var minorBrush = new SolidColorBrush(Color.FromArgb(255, 195, 195, 195));
        majorGridPen = new Pen(majorBrush, 1.0);
        minorGridPen = new Pen(minorBrush, 1.0);
        AffectsRender<GridVisualHost>(ViewportTransformProperty);
        // SizeChanged += (s, e) => UpdateDrawingVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (ViewportTransform is MatrixTransform transform)
        {
            var m = transform.Matrix;
            UpdateDrawingVisual(context,m.M31, m.M32, m.M11);
        }
        base.Render(context);
    }
    

    #endregion

    #region Private Class Helper and Event Handlers

    private void UpdateDrawingVisual()
    {
        InvalidateVisual();
    }


    private void UpdateDrawingVisual(DrawingContext context,double x, double y, double zoom)
    {
        #region Scale Adjustment

        // Bring scale factor to within zoom boundaries.
        if(zoom == 0) return;
        
        var localScale = zoom;
        while (localScale * MajorGridLineSpacing < MinMajorGridSpacing)
            localScale = localScale * ScaleFactor;
        while (localScale * MajorGridLineSpacing > MaxMajorGridSpacing)
            localScale = localScale / ScaleFactor;

        #endregion

        #region Positional Adjustment

        // The scale is know, adjust the top-left corner.
        var scaledMajorGridSpacing = localScale * MajorGridLineSpacing;
        var startX = NormalizeStartPoint(x, scaledMajorGridSpacing);
        var startY = NormalizeStartPoint(y, scaledMajorGridSpacing);

        #endregion

        var unitGrid = (localScale * (MajorGridLineSpacing / MinorDivisions));

        #region Vertical grid lines

        int counter = 0;
        var pointOne = new Point(startX, 0.0);
        var pointTwo = new Point(startX, Bounds.Height);

        while (true)
        {
            var isMajorGridLine = ((counter % MinorDivisions) == 0);

            var offset = unitGrid * counter++;
            if (offset > Bounds.Width + scaledMajorGridSpacing)
                break;

            var pen = isMajorGridLine ? majorGridPen : minorGridPen;
            pointOne = new Point(startX + offset, pointOne.Y);
            pointTwo = new Point(pointOne.X, pointTwo.Y);
            context.DrawLine(pen, pointOne, pointTwo);
        }

        #endregion

        #region Horizontal grid lines

        counter = 0;
        pointOne = new Point(0.0, startY);
        pointTwo = new Point(Bounds.Width, startY);

        while (true)
        {
            var isMajorGridLine = ((counter % MinorDivisions) == 0);

            var offset = unitGrid * counter++;
            if (offset > Bounds.Height + scaledMajorGridSpacing)
                break;

            var pen = isMajorGridLine ? majorGridPen : minorGridPen;
            pointOne = new Point(pointOne.X, startY + offset);
            pointTwo = new Point(pointTwo.X, pointOne.Y);
            context.DrawLine(pen, pointOne, pointTwo);
        }

        #endregion
    }

    private double NormalizeStartPoint(double value, double scaledMajorGridSpacing)
    {
        if (value > 0)
        {
            // Assuming after applying scale factor, the major grid lines 
            // are 10px apart. If the current WorkspaceModel.X is 24px, then 
            // 
            //      v = floor(24/10) = floor(2.4) = 2
            //      w = 24 - 2 x 10 = 4
            // 
            // We could start drawing the major grid line from 4px, but that 
            // leaves a gap at the left edge. So it would be nice if major 
            // grid line starts from 4 - 10 = -6px, so that it appears that 
            // the left-most grid line is beyond the left edge.
            // 
            var v = ((int)Math.Floor(value / scaledMajorGridSpacing));
            var w = (value - (v * scaledMajorGridSpacing));
            value = w - scaledMajorGridSpacing;
        }
        else if (value < -scaledMajorGridSpacing)
        {
            // Assuming after applying scale factor, the major grid lines 
            // are 10px apart. If the current WorkspaceModel.X is -24px, then 
            // 
            //      value = abs(-24) = 24
            //      v = floor(24/10) = floor(2.4) = 2
            //      w = 24 - 2 x 10 = 4
            //      value = -w = -4
            // 
            value = Math.Abs(value);
            var v = ((int)Math.Floor(value / scaledMajorGridSpacing));
            var w = (value - (v * scaledMajorGridSpacing));
            value = -w;
        }

        return value;
    }

    #endregion
}