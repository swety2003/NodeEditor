using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace NodeEditor.Controls;

public partial class SampleNodeEditor
{
    
    protected class StateMachine
{
    #region Private Class Data Members

    private enum State
    {
        Idle,
        Panning,
        WindowSelection,
        Dragging,
        Connecting,
    }

    private State currentState = State.Idle;
    private Point mouseDownPos = new();
    private SampleNodeEditor owningWorkspace;

    #endregion

    public StateMachine(SampleNodeEditor workspaceView)
    {
        owningWorkspace = workspaceView;
    }

    private bool CanTransition(State newState)
    {
        if (!owningWorkspace.EditorOptions.HasFlag(EditorOptions.DragMove) && newState == State.Dragging)
        {
            return false;
        }
        if (!owningWorkspace.EditorOptions.HasFlag(EditorOptions.PanAndZoom) && newState == State.Panning)
        {
            return false;
        }
        if (!owningWorkspace.EditorOptions.HasFlag(EditorOptions.Connection) && newState == State.Connecting)
        {
            return false;
        }
        return true;
    }

    private void GotoState(State newState)
    {
        if (currentState == newState || !CanTransition(newState))
        {
            return;
        }
        
        

        if (newState == State.Dragging)
        {
            owningWorkspace.ItemDragStart(true);
        }

        // owningWorkspace.StateDebugText.Text = newState.ToString();
        // todo
        currentState = newState;
    }

    private bool hitItem = false;
    private bool hitConnector = false;

    private bool CanIgnore(Visual? visualSource)
    {
        Visual? current = visualSource;
        while (true)
        {
            // update parent
            if (current == null || current == owningWorkspace)
            {
                return false;
            }
            if (EditorBehaviourOptions.GetIgnoreHitTest(current as Control) == true)
            {
                return true;
            }
            current = current?.GetVisualParent();
        }
        return false;
    }

    public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetPosition(owningWorkspace);
        mouseDownPos = p;
        var visualSource = e.Source as Visual;
        var item = visualSource.FindAncestorOfType<CanvasItem>();
        var connector = visualSource.FindAncestorOfType<Connector>();
        hitItem = item != null;
        hitConnector = connector != null;
        if (item != null)
        {
            owningWorkspace.BringToFront(item);
        }
        // todo 
        if (CanIgnore(visualSource))
        {
            return;
        }
        
        // Console.WriteLine("P "+visualSource);
        
        if (currentState == State.Idle)
        {
            // select first
            if (e.Properties.IsLeftButtonPressed)
            {
                if (item != null)
                {
                    if (item.IsSelected == false)
                    {
                        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
                        owningWorkspace.SelectItem(item,shift);
                        // owningWorkspace.CanvasItemsControl .UpdateSelection(item, true,shift);
                    }
                    
                    // hit item start drag item
                    // owningWorkspace.ItemDragStart();

                }
                else
                {
                    // hit nothing
                    owningWorkspace.ClearSelection();
                }

                if (connector != null)
                {
                    // owningWorkspace.PendingConnection.Source = connector.Anchor;
                    owningWorkspace.StartPendingConnection(connector);
                }
            }

            if (e.Properties.IsMiddleButtonPressed)
            {
                owningWorkspace.ZoomDragStart();
            }
        }
    }
    public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var p = e.GetPosition(owningWorkspace);
        
        if (currentState == State.Dragging)
        {
            owningWorkspace.ItemDragEnd();
            e.Pointer.Capture(null);
            GotoState(State.Idle);
        }else if (currentState == State.Panning)
        {
            e.Pointer.Capture(null);
            GotoState(State.Idle);
        }else if (currentState == State.WindowSelection)
        {
            e.Pointer.Capture(null);
            owningWorkspace.UpdateSelectionBox();
            GotoState(State.Idle);
        }else if (currentState == State.Connecting)
        {
            e.Pointer.Capture(null);
            // test finish
            var elements = owningWorkspace.GetInputElementsAt(p)
                .OfType<Control>();
            owningWorkspace.SnapPendingConnection(elements,new Point(0,0));
            // todo make connection
            owningWorkspace.FinishPendingConnection();
            
            // owningWorkspace.UpdatePendingConnection(); = false;
            GotoState(State.Idle);
        }
    }

    private int threshold = 3;
    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // todo ignore events
        if (CanIgnore(e.Source as Visual))
        {
            return;
        }
        
        var p = e.GetPosition(owningWorkspace);
        Vector delta = p - mouseDownPos;
        var scale = owningWorkspace.ViewTransform.Matrix.M11;
        if (Math.Abs(delta.X) > threshold || Math.Abs(delta.Y) > threshold)
        {
            // drag start
            if (e.Properties.IsLeftButtonPressed)
            {
                if (hitConnector)
                {
                    GotoState(State.Connecting);
                }
                else if (hitItem)
                {
                    GotoState(State.Dragging);
                }
                else
                {
                    GotoState(State.WindowSelection);
                }
                
            }else if (e.Properties.IsMiddleButtonPressed)
            {
                GotoState(State.Panning);
            }
        }

        if (currentState == State.Dragging)
        {
            e.Pointer.Capture(sender as InputElement);
            owningWorkspace.ItemDragMove(delta / scale);
        }

        if (currentState == State.WindowSelection)
        {
            e.Pointer.Capture(sender as InputElement);
            owningWorkspace.UpdateSelectionBox(mouseDownPos,delta);
        }

        if (currentState == State.Panning)
        {
            e.Pointer.Capture(sender as InputElement);
            owningWorkspace.ZoomDragMove(delta);
        }

        if (currentState == State.Connecting)
        {
            e.Pointer.Capture(sender as InputElement);
            var elements = owningWorkspace.GetInputElementsAt(p)
                .OfType<Control>();
            owningWorkspace.SnapPendingConnection(elements,owningWorkspace.ViewTransform.Matrix.Invert().Transform(p));
            // Console.WriteLine("M "+e.Source);

        }
    }
    
    
}
}