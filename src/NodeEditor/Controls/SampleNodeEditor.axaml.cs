using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace NodeEditor.Controls;

public partial class SampleNodeEditor : TemplatedControl
{
    private StateMachine _stateMachine;
    public SampleNodeEditor()
    {
        _stateMachine = new StateMachine(this);
    }
    
    #region Control Properties

    public static readonly StyledProperty<Point> MouseLocationProperty = AvaloniaProperty.Register<SampleNodeEditor, Point>(
        nameof(MouseLocation),defaultBindingMode:BindingMode.OneWayToSource);

    public Point MouseLocation
    {
        get => GetValue(MouseLocationProperty);
        set => SetValue(MouseLocationProperty, value);
    }

    public static readonly StyledProperty<IBrush> GridLineBrushProperty = AvaloniaProperty.Register<SampleNodeEditor, IBrush>(
        nameof(GridLineBrush));

    public IBrush GridLineBrush
    {
        get => GetValue(GridLineBrushProperty);
        set => SetValue(GridLineBrushProperty, value);
    }
    
    public static readonly StyledProperty<MatrixTransform> ViewTransformProperty = AvaloniaProperty.Register<SampleNodeEditor, MatrixTransform>(
        nameof(ViewTransform), new MatrixTransform(), coerce: (o, transform) => transform ?? new MatrixTransform());

    public MatrixTransform ViewTransform
    {
        get => GetValue(ViewTransformProperty);
        set => SetValue(ViewTransformProperty, value);
    }

    public static readonly StyledProperty<IList> SelectedNodesProperty = AvaloniaProperty.Register<SampleNodeEditor, IList>(
        nameof(SelectedNodes));

    public IList SelectedNodes
    {
        get => GetValue(SelectedNodesProperty);
        set => SetValue(SelectedNodesProperty, value);
    }

    public static readonly StyledProperty<IEnumerable> NodesProperty = AvaloniaProperty.Register<SampleNodeEditor, IEnumerable>(
        nameof(Nodes));

    public IEnumerable Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    public static readonly StyledProperty<IEnumerable> ConnectionsProperty = AvaloniaProperty.Register<SampleNodeEditor, IEnumerable>(
        nameof(Connections));

    public IEnumerable Connections
    {
        get => GetValue(ConnectionsProperty);
        set => SetValue(ConnectionsProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate> NodeTemplateProperty = AvaloniaProperty.Register<SampleNodeEditor, IDataTemplate>(
        nameof(NodeTemplate));

    public IDataTemplate NodeTemplate
    {
        get => GetValue(NodeTemplateProperty);
        set => SetValue(NodeTemplateProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate> ConnectionTemplateProperty = AvaloniaProperty.Register<SampleNodeEditor, IDataTemplate>(
        nameof(ConnectionTemplate));

    public IDataTemplate ConnectionTemplate
    {
        get => GetValue(ConnectionTemplateProperty);
        set => SetValue(ConnectionTemplateProperty, value);
    }

    public static readonly StyledProperty<ControlTheme> ItemContainerThemeProperty = AvaloniaProperty.Register<SampleNodeEditor, ControlTheme>(
        nameof(ItemContainerTheme));

    public ControlTheme ItemContainerTheme
    {
        get => GetValue(ItemContainerThemeProperty);
        set => SetValue(ItemContainerThemeProperty, value);
    }

    public static readonly StyledProperty<ControlTheme> PendingConnectionThemeProperty = AvaloniaProperty.Register<SampleNodeEditor, ControlTheme>(
        nameof(PendingConnectionTheme));

    public ControlTheme PendingConnectionTheme
    {
        get => GetValue(PendingConnectionThemeProperty);
        set => SetValue(PendingConnectionThemeProperty, value);
    }
    
    #endregion
    
    #region Controls

    private CanvasItemsControl _canvasItemsControl;
    private PendingConnection _pendingConnection;
    private ZoomBorder _zoomBorder;
    private Rectangle _selectionBox;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _canvasItemsControl = e.NameScope.Find<CanvasItemsControl>("PART_CanvasItemsControl");
        _pendingConnection = e.NameScope.Find<PendingConnection>("PART_PendingConnection");
        _zoomBorder = e.NameScope.Find<ZoomBorder>("PART_ZoomBorder");
        _selectionBox = e.NameScope.Find<Rectangle>("PART_SelectionBox");
    }

    private void BringToFront(CanvasItem item)
    {
        foreach (var item1 in _canvasItemsControl.AllContainers)
        {
            item1.ZIndex = 0;
        }
        item.ZIndex = 1;
    }

    private void SelectItem(CanvasItem item, bool keepSelection)
    {
        if (!keepSelection)
        {
            _canvasItemsControl.Selection.Clear();
        }
        _canvasItemsControl.Selection.Select(_canvasItemsControl.IndexFromContainer(item));
    }
    private void ClearSelection()
    {
        _canvasItemsControl.Selection.Clear();
    }

    private void ItemDragStart(bool raiseEvent=false)
    {
        var selected = _canvasItemsControl.SelectedContainers;
        foreach (var container in selected)
        {
            container.DragStart();
        }

        if (raiseEvent)
        {
            NodesDragStarted?.Invoke(this, EventArgs.Empty);
        }
    }
    private void ItemDragMove(Vector delta)
    {
        foreach (var container in _canvasItemsControl.SelectedContainers)
        {
            container.DragMove(delta);
        }
    }
    private void ItemDragEnd()
    {
        foreach (var container in _canvasItemsControl.SelectedContainers)
        {
            container.DragEnd();
        }
        NodesDragFished?.Invoke(this, EventArgs.Empty);
    }

    private void ZoomDragMove(Vector delta)
    {
        _zoomBorder.DragMove(delta);
    }
    private void ZoomDragStart()
    {
        _zoomBorder.DragStart();
    }
    
    
    private void UpdateSelectionBox(Point mouseDownPos, Vector delta, bool visible=true)
    {
        Point topLeft = mouseDownPos;
        _selectionBox.IsVisible = visible;
        if (delta.X <0)
        {
            topLeft = new Point(topLeft.X + delta.X, topLeft.Y);
        }

        if (delta.Y < 0)
        {
            topLeft = new Point(topLeft.X, topLeft.Y + delta.Y);
        }
        
        var rect = new Rect(topLeft.X, topLeft.Y,Math.Abs(delta.X), Math.Abs(delta.Y));
        Canvas.SetLeft(_selectionBox,rect.X);
        Canvas.SetTop(_selectionBox,rect.Y);
        _selectionBox.Width = rect.Width;
        _selectionBox.Height = rect.Height;
        
        // var word_tl = ZoomBorder.CanvasTransform.Matrix.Transform(topLeft);
        // var word_rect = new Rect(word_tl, new Size(rect.Width / ZoomBorder.Scale , rect.Height  / ZoomBorder.Scale));
        // foreach (var canvasItem in CanvasItemsControl.AllContainers.Where(x => word_rect.Contains(new Point(x.Top,x.Left))))
        // {
        //     CanvasItemsControl.Selection.Select(CanvasItemsControl.IndexFromContainer(canvasItem));
        // };
        UpdateSelectionBox(true,visible);
    }
    private void UpdateSelectionBox(bool select = true,bool visible=false)
    {
        var word_tl = ViewTransform.Matrix.Invert().Transform(new Point(
            Canvas.GetLeft(_selectionBox),Canvas.GetTop(_selectionBox)));
        var word_rect = new Rect(word_tl, new Size(_selectionBox.Width / _zoomBorder.Scale , _selectionBox.Height  / _zoomBorder.Scale));
        // Console.WriteLine(word_rect.ToString());
        foreach (var canvasItem in _canvasItemsControl.AllContainers)
        {
            // if (!word_rect.Contains(new Point(canvasItem.Top,canvasItem.Left))) return;
            if (canvasItem.IsSelected || !word_rect.Contains(canvasItem.Bounds)) continue;
            _canvasItemsControl.Selection.Select(_canvasItemsControl.IndexFromContainer(canvasItem));
        };
        _selectionBox.IsVisible = visible;
    }
    
    
    private void SnapPendingConnection(IEnumerable<Control> elements,Point fallbackPoint)
    {
        _pendingConnection.IsVisible = true;
        foreach (var element in elements)
        {
            if (element.FindAncestorOfType<Connector>() is {  } c)
            {
                _pendingConnection.Target = c.Anchor;
                PendingConnectionUpdated?.Invoke(this,c.DataContext);
                return;
            }
        }

        _pendingConnection.Target = fallbackPoint;
        PendingConnectionUpdated?.Invoke(this,null);
    }
    

    private void StartPendingConnection(Connector connector)
    {
        _pendingConnection.Source = connector.Anchor;
        PendingConnectionStarted?.Invoke(this,connector.DataContext);
    }
    private void FinishPendingConnection()
    {
        PendingConnectionFinished?.Invoke(this,EventArgs.Empty);
        _pendingConnection.IsVisible = false;
    }

    #endregion

    #region Events

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _stateMachine.OnPointerPressed(this,e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _stateMachine.OnPointerMoved(this,e);
        MouseLocation = ViewTransform.Matrix.Invert().Transform(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _stateMachine.OnPointerReleased(this,e);
    }
    
    #endregion
    
    
    public event EventHandler<object?>? PendingConnectionStarted; 
    public event EventHandler<object?>? PendingConnectionUpdated; 
    public event EventHandler? PendingConnectionFinished;
    public event EventHandler? NodesDragStarted;
    public event EventHandler? NodesDragFished;

}
