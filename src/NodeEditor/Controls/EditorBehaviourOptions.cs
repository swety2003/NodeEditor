using Avalonia;
using Avalonia.Controls;

namespace NodeEditor.Controls;

public class EditorBehaviourOptions : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IgnoreHitTestProperty =
        AvaloniaProperty.RegisterAttached<EditorBehaviourOptions, Control, bool>("IgnoreHitTest");

    public static void SetIgnoreHitTest(Control obj, bool value) => obj.SetValue(IgnoreHitTestProperty, value);
    public static bool GetIgnoreHitTest(Control obj) => obj.GetValue(IgnoreHitTestProperty);
}