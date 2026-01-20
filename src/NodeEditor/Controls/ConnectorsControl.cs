using Avalonia.Controls;

namespace NodeEditor.Controls;


public class ConnectorsControl: ItemsControl
{
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new Connector();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<Connector>(item,out recycleKey);
    }
}