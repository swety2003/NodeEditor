namespace NodeEditor.Controls;

[Flags]
public enum EditorOptions
{
    All = 0b111,
    PanAndZoom = 1,
    DragMove = 2,
    Connection = 4,
}