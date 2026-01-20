using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using NodeEditor.Utils;

namespace NodeEditor.Controls;


public class ZoomBorder: Border
{
    public static readonly StyledProperty<MatrixTransform> CanvasTransformProperty = AvaloniaProperty.Register<ZoomBorder, MatrixTransform>(
        nameof(CanvasTransform), new MatrixTransform(Matrix.Identity));

    public MatrixTransform CanvasTransform
    {
        get => GetValue(CanvasTransformProperty);
        set => SetValue(CanvasTransformProperty, value);
    }

    public double Scale => CanvasTransform.Matrix.M11;
    

    public ZoomBorder()
    {
    }

    private Point _startPoint;
    private Matrix _matrix;

    #region Child Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        this.PointerWheelChanged += OnPointerWheelChanged;
        // this.PointerMoved += OnPointerMoved;
        // this.PointerPressed += OnPointerPressed;
        // this.PointerReleased += OnPointerReleased;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Pointer.Captured != null || e.KeyModifiers.HasFlag(KeyModifiers.Control) == false)
        {
            return;
        }
        
        const double scale1 = 1.1;
        const double scale2 = 0.9;
        
        var p = e.GetPosition(this);
        var target = CanvasTransform.Matrix.Invert().Transform(p);
        // Console.WriteLine(target.X + "," + target.Y);
        var delta = e.Delta.Y;
        if (delta > 0)
        {
            CanvasTransform = MatrixHelper.ScaleAtPrepend(CanvasTransform.Matrix,scale1, scale1,target.X,target.Y).ToMatrixTransform();
        }
        else
        {
            CanvasTransform = MatrixHelper.ScaleAtPrepend(CanvasTransform.Matrix,scale2, scale2,target.X,target.Y).ToMatrixTransform();
        }
        
        ClearValue(CanvasTransformProperty);
    }
    
    #endregion

    public void DragStart()
    {
        _matrix = CanvasTransform.Matrix;
    }
    
    public void DragMove(Vector delta, bool relative = false)
    {
        if (relative)
        {
            CanvasTransform = (CanvasTransform.Matrix * MatrixHelper.Translate(delta.X, delta.Y)).ToMatrixTransform();
        }
        else
        {
            CanvasTransform = (_matrix * MatrixHelper.Translate(delta.X, delta.Y)).ToMatrixTransform();
        }
        ClearValue(CanvasTransformProperty);
    }
}