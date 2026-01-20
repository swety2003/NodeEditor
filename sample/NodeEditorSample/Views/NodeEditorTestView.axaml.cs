using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NodeEditorSample.ViewModels;

namespace NodeEditorSample.Views;

public partial class NodeEditorTestView : UserControl
{
    NodeEditorTestViewModel vm => DataContext as NodeEditorTestViewModel;
    public NodeEditorTestView()
    {
        InitializeComponent();
        DataContext = new NodeEditorTestViewModel();
        Editor.PendingConnectionUpdated += EditorOnPendingConnectionUpdated;
        Editor.PendingConnectionStarted += EditorOnPendingConnectionStarted;
        Editor.PendingConnectionFinished += EditorOnPendingConnectionFinished;
        Editor.NodesDragStarted += EditorOnNodesDragStarted;
        Editor.NodesDragFished += EditorOnNodesDragFished;
    }
    
    
    private void EditorOnNodesDragFished(object? sender, EventArgs e)
    {
        vm.OnNodesMove(false);
    }

    private void EditorOnNodesDragStarted(object? sender, EventArgs e)
    {
        vm.OnNodesMove(true);
    }

    private void EditorOnPendingConnectionFinished(object? sender, EventArgs e)
    {
        if (vm.CanCreateConnection())
        {
            vm.CreateConnection(vm.PendingConnection.Source, vm.PendingConnection.Target);
        }
    }

    private void EditorOnPendingConnectionStarted(object? sender, object? e)
    {
        vm.PendingConnection.Source = e as ConnectorViewModel;
    }

    private void EditorOnPendingConnectionUpdated(object? sender, object? e)
    {
        if (vm.PendingConnection.Source != null)
        {
            vm.PendingConnection.Target = e as ConnectorViewModel;
            vm.CanCreateConnection();
        }
    }
}