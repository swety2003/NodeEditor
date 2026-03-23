namespace NodeEditor.Controls;

[Flags]
public enum EditorOptions
{
    All = 0b1111,
    PanAndZoom = 1,
    DragMove = 2,
    Connection = 4,
    AutoPannngOnEdge = 8,
}