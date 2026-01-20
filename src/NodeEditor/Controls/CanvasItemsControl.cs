using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;

namespace NodeEditor.Controls;


public class CanvasItemsControl: SelectingItemsControl
{
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new Canvas());
    
    public static readonly new DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty =
        SelectingItemsControl.SelectedItemsProperty;
    
    public static readonly new DirectProperty<SelectingItemsControl, ISelectionModel> SelectionProperty =
        SelectingItemsControl.SelectionProperty;
    public static readonly new StyledProperty<SelectionMode> SelectionModeProperty =
        SelectingItemsControl.SelectionModeProperty;

    static CanvasItemsControl()
    {
        // ItemsPanelProperty.OverrideDefaultValue<CanvasItemsControl>(DefaultPanel);
        SelectionModeProperty.OverrideDefaultValue<CanvasItemsControl>(SelectionMode.Multiple);
    }
    
    
    /// <inheritdoc cref="SelectingItemsControl.SelectedItems"/>
    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    /// <inheritdoc cref="SelectingItemsControl.Selection"/>
    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }
    
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    public IList<CanvasItem> SelectedContainers => AllContainers.Where(x=>x.IsSelected).ToList();
    public IList<CanvasItem> AllContainers => Enumerable.Range(0,Items.Count).Select(ContainerFromIndex).OfType<CanvasItem>().ToList();
    
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CanvasItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CanvasItem>(item, out recycleKey);
    }
}