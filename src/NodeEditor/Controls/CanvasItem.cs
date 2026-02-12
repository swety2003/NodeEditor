using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace NodeEditor.Controls;


[PseudoClasses(":selected")]
public class CanvasItem: ContentControl, ISelectable
{
    /// <summary>
    /// Defines the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<CanvasItem>();
    
    
    /// <summary>
    /// Gets or sets the selection state of the item.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSelectedProperty)
        {
            PseudoClasses.Set(":selected", (bool)change.NewValue);
        }

        if (change.Property == Canvas.LeftProperty || change.Property == Canvas.TopProperty)
        {
            var node = this.FindDescendantOfType<Node>();
            node?.UpdateAnchors();        
        }
    }

    public static readonly StyledProperty<int> MoveStepProperty = AvaloniaProperty.Register<CanvasItem, int>(
        nameof(MoveStep),5);

    public int MoveStep
    {
        get => GetValue(MoveStepProperty);
        set => SetValue(MoveStepProperty, value);
    }

    static CanvasItem()
    {
        Canvas.LeftProperty.OverrideDefaultValue<CanvasItem>(0);
        Canvas.TopProperty.OverrideDefaultValue<CanvasItem>(0);
        
        SelectableMixin.Attach<CanvasItemsControl>(IsSelectedProperty);
    }
    
    /// <summary>
    /// https://github.com/AvaloniaUI/Avalonia/issues/6684
    /// </summary>
    public double Left
    {
        get => Canvas.GetLeft(this);
        set
        {
            Canvas.SetLeft(this, value);
            ClearValue(Canvas.LeftProperty);
        }
    }

    public double Top
    {
        get => Canvas.GetTop(this);
        set
        {
            Canvas.SetTop(this, value);
            ClearValue(Canvas.TopProperty);
        }
    }

    #region DragMove

    private Point _dragStart;
    public void DragStart()
    {
        _dragStart = new Point(Left, Top);
    }
    public void DragMove(Vector vector)
    {
        var x = _dragStart.X + vector.X;
        var y = _dragStart.Y + vector.Y;
        
        Left = ((int)x / MoveStep) * MoveStep;
        Top = ((int)y / MoveStep) * MoveStep;
    }
    
    public void DragCancel()
    {
        Left = _dragStart.X;
        Top = _dragStart.Y;
    }
    
    public void DragEnd()
    {
        // todo
        // Left = ((int)Left / MoveStep) * MoveStep;
        // Top = ((int)Top / MoveStep) * MoveStep;
    }
    
    

    #endregion
}