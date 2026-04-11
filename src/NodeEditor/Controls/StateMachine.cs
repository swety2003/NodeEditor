using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using NodeEditor.Utils;

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
        private Point mouseDownPos;
        private Vector viewDragSum;
        // private SampleNodeEditor owningWorkspace;

        #endregion

        private bool CanTransition(EditorOptions options,State newState)
        {
            if (!options.HasFlag(EditorOptions.DragMove) && newState == State.Dragging)
            {
                return false;
            }

            if (!options.HasFlag(EditorOptions.PanAndZoom) && newState == State.Panning)
            {
                return false;
            }

            if (!options.HasFlag(EditorOptions.Connection) && newState == State.Connecting)
            {
                return false;
            }

            return true;
        }

        private void GotoState(SampleNodeEditor owningWorkspace,State newState)
        {
            if (currentState == newState || !CanTransition(owningWorkspace.EditorOptions,newState))
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

        private bool CanIgnore(SampleNodeEditor? owningWorkspace, Visual? visualSource)
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

        public void OnPointerPressed(SampleNodeEditor? owningWorkspace, PointerPressedEventArgs e)
        {
            var p = e.GetPosition(owningWorkspace);
            mouseDownPos = p;
            viewDragSum = new Point();
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
            if (CanIgnore(owningWorkspace,visualSource))
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
                            owningWorkspace.SelectItem(item, shift);
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

        public void OnPointerReleased(SampleNodeEditor? owningWorkspace, PointerReleasedEventArgs e)
        {
            var p = e.GetPosition(owningWorkspace);

            if (currentState == State.Dragging)
            {
                owningWorkspace.ItemDragEnd();
                e.Pointer.Capture(null);
                GotoState(owningWorkspace,State.Idle);
            }
            else if (currentState == State.Panning)
            {
                e.Pointer.Capture(null);
                GotoState(owningWorkspace,State.Idle);
            }
            else if (currentState == State.WindowSelection)
            {
                e.Pointer.Capture(null);
                owningWorkspace.UpdateSelectionBox();
                GotoState(owningWorkspace,State.Idle);
            }
            else if (currentState == State.Connecting)
            {
                e.Pointer.Capture(null);
                // test finish
                var elements = owningWorkspace.GetInputElementsAt(p)
                    .OfType<Control>();
                owningWorkspace.SnapPendingConnection(elements, new Point(0, 0));
                // todo make connection
                owningWorkspace.FinishPendingConnection();

                // owningWorkspace.UpdatePendingConnection(); = false;
                GotoState(owningWorkspace,State.Idle);
            }
        }

        private int threshold = 3;
        private Point lastMovePos;

        public void OnPointerMoved(SampleNodeEditor? owningWorkspace, PointerEventArgs e)
        {
            var p = e.GetPosition(owningWorkspace);

            if (CanIgnore(owningWorkspace,e.Source as Visual))
            {
                return;
            }

            Vector delta = p - mouseDownPos;
            var scale = owningWorkspace.ViewTransform.Matrix.M11;
            if (Math.Abs(delta.X) > threshold || Math.Abs(delta.Y) > threshold)
            {
                // drag start
                if (e.Properties.IsLeftButtonPressed)
                {
                    if (hitConnector)
                    {
                        GotoState(owningWorkspace,State.Connecting);
                    }
                    else if (hitItem)
                    {
                        GotoState(owningWorkspace,State.Dragging);
                    }
                    else
                    {
                        GotoState(owningWorkspace,State.WindowSelection);
                    }
                }
                else if (e.Properties.IsMiddleButtonPressed)
                {
                    GotoState(owningWorkspace,State.Panning);
                }


                if (currentState is State.Connecting or State.Dragging &&
                    owningWorkspace.EditorOptions.HasFlag(EditorOptions.AutoPannngOnEdge))
                {
                    var delta1 = MoveView(owningWorkspace.Bounds,p, p - lastMovePos);
                    viewDragSum += delta1;
                    if (delta1.X != 0 || delta1.Y != 0)
                    {
                        owningWorkspace.ViewTransform =
                            (owningWorkspace.ViewTransform.Matrix * MatrixHelper.Translate(-delta1.X, -delta1.Y))
                            .ToMatrixTransform();
                    }

                    if (viewDragSum.X != 0 || viewDragSum.Y != 0)
                    {
                        delta += viewDragSum / scale;
                    }
                }
            }


            if (currentState == State.Dragging)
            {
                e.Pointer.Capture(owningWorkspace);
                owningWorkspace.ItemDragMove(delta / scale);
            }

            if (currentState == State.WindowSelection)
            {
                e.Pointer.Capture(owningWorkspace);
                owningWorkspace.UpdateSelectionBox(mouseDownPos, delta);
            }

            if (currentState == State.Panning)
            {
                e.Pointer.Capture(owningWorkspace);
                owningWorkspace.ZoomDragMove(delta);
            }

            if (currentState == State.Connecting)
            {
                e.Pointer.Capture(owningWorkspace);
                var elements = owningWorkspace.GetInputElementsAt(p)
                    .OfType<Control>();
                owningWorkspace.SnapPendingConnection(elements,
                    owningWorkspace.ViewTransform.Matrix.Invert().Transform(p));
                // Console.WriteLine("M "+e.Source);
            }


            lastMovePos = p;
        }


        #region ViewMove

        private Vector MoveView(Rect rect,Point p, Vector delta)
        {
            // var rect = owningWorkspace.Bounds;
            // 计算移动向量
            var moveX = CalculateMoveAmount(p.X, rect.Width, delta.X);
            var moveY = CalculateMoveAmount(p.Y, rect.Height, delta.Y);
            return new Vector(moveX, moveY) * 0.1;
        }

        private float moveThreshold = 0.1f;

        private double CalculateMoveAmount(double position, double dimension, double delta)
        {
            float threshold = (float)dimension * moveThreshold;
            float minEdge = threshold;
            float maxEdge = (float)dimension - threshold;
            if (position - minEdge < 0 && delta < 0)
            {
                return (position - minEdge);
            }

            if (position - maxEdge > 0 && delta > 0)
            {
                return (position - maxEdge);
            }

            return 0;
        }

        #endregion
    }
}