using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace NodeEditor.Controls;

[TemplatePart("PART_ConnectorItemsControl",typeof(ItemsControl))]
public class Node : TemplatedControl
{
    public virtual void UpdateAnchors()
    {
        var ports = Enumerable.Range(0, _inputsItemsControl.ItemCount).Select(x => _inputsItemsControl.ContainerFromIndex(x))
            .Concat(Enumerable.Range(0, _outputsItemsControl.ItemCount).Select(x => _outputsItemsControl.ContainerFromIndex(x)));
        foreach (var connector in ports.OfType<Connector>())
        {
            connector.UpdateAnchor();
        }
    }

    private ItemsControl _inputsItemsControl;
    private ItemsControl _outputsItemsControl;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _inputsItemsControl = e.NameScope.Find<ItemsControl>("PART_Inputs");
        _outputsItemsControl = e.NameScope.Find<ItemsControl>("PART_Outputs");
    }

    public static readonly StyledProperty<IList> OutputsProperty = AvaloniaProperty.Register<Node, IList>(
        nameof(Outputs));

    public IList Outputs
    {
        get => GetValue(OutputsProperty);
        set => SetValue(OutputsProperty, value);
    }

    private IList _inputs;

    public static readonly DirectProperty<Node, IList> InputsProperty = AvaloniaProperty.RegisterDirect<Node, IList>(
        nameof(Inputs), o => o.Inputs, (o, v) => o.Inputs = v);

    public IList Inputs
    {
        get => _inputs;
        set => SetAndRaise(InputsProperty, ref _inputs, value);
    }

    public static readonly StyledProperty<ControlTheme> ConnectorThemeProperty = AvaloniaProperty.Register<Node, ControlTheme>(
        nameof(ConnectorTheme));

    public ControlTheme ConnectorTheme
    {
        get => GetValue(ConnectorThemeProperty);
        set => SetValue(ConnectorThemeProperty, value);
    }

    
    
    public static readonly StyledProperty<object> HeaderProperty = AvaloniaProperty.Register<Node, object>(
        nameof(Header));
    
    
    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<Node, object>(
        nameof(Content));

    [Content]
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}