using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeEditor.Controls;
using NodeEditorSample.UndoRedo;

namespace NodeEditorSample.ViewModels;

public partial class NodeEditorTestViewModel: ObservableObject
{
    [ObservableProperty]EditorOptions options = EditorOptions.All;

    public bool CanDragMove
    {
        get => Options.HasFlag(EditorOptions.DragMove);
        set
        {
            if (value)
                Options |= EditorOptions.DragMove;
            else
                Options &= ~EditorOptions.DragMove;
        }
    }
    public bool CanConnect
    {
        get => Options.HasFlag(EditorOptions.Connection);
        set
        {
            if (value)
                Options |= EditorOptions.Connection;
            else
                Options &= ~EditorOptions.Connection;
        }
    }
    
    public AvaloniaList<NodeViewModel> Nodes { get; set; } = [];
    public AvaloniaList<NodeViewModel> SelectedNodes { get; set; } = [];
    public AvaloniaList<ConnectionViewModel> Connections { get; set; } = [];
    public PendingConnectionW PendingConnection { get; set; } = new();
    
    public NodeEditorTestViewModel()
    {
        
        UndoCommand = new RelayCommand(Undo, () => UndoRedo.CanUndo);

        RedoCommand = new RelayCommand(Redo, () => UndoRedo.CanRedo);
        
        var a = new NodeViewModel()
        {
            Title = "a",
        };
        var b = new NodeViewModel()
        {
            Title = "b",
            X = 200,
        };

        a.AddInput("in_0");
        a.AddOutput("out_0");
        a.AddOutput("out_1");

        b.AddInput("in_0");
        b.AddOutput("out_0");
        
        Nodes.Add(a);
        Nodes.Add(b);
        
    }

    [RelayCommand]
    void NewNode()
    {
        
        var a = new NodeViewModel()
        {
            Title = Guid.NewGuid().ToString().Substring(8),
            X = Random.Shared.Next(300),
            Y = Random.Shared.Next(300),
        };
        a.AddInput("in_0");
        a.AddOutput("out_0");
        var act = new DelegateAction(() => Nodes.Add(a), () => Nodes.Remove(a), "添加");
        UndoRedo.ExecuteAction(act);

    }


    public bool CanCreateConnection()
    {
        if (PendingConnection.Target == null || PendingConnection.Source == PendingConnection.Target)
        {
            PendingConnection.Text = "拖动以连接";
            return false;
        }

        if (PendingConnection.Target.Output == PendingConnection.Source.Output)
        {
            PendingConnection.Text = "连接模式非法";
            return false;
        }

        if (PendingConnection.Source.Parent == PendingConnection.Target.Parent)
        {
            PendingConnection.Text = "不能连接自身";
            return false;
        }
        
        PendingConnection.Text = "松开左键以建立连接";
        return true;
    }

    #region Graph Operation
    public IActionsHistory UndoRedo { get; } = new ActionsHistory();

    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    
    private void Undo()
    {
        UndoRedo.Undo();
    }

    private void Redo()
    {
        UndoRedo.Redo();
    }

    
    public void CreateConnection(ConnectorViewModel left, ConnectorViewModel right)
    {
        
        var (input, output) = !left.Output ? (left, right) : (right, left);
        var newCon = new ConnectionViewModel()
        {
            Source = output,
            Target = input,
        };
        var action = new DelegateAction(() =>
        {
            Connections.Add(newCon);
            OnConnectionAdded(newCon);
        }, () =>
        {
            Connections.Remove(newCon);
            OnConnectionRemoved(newCon);
        }, "连接操作");
        
        UndoRedo.ExecuteAction(action);
        
    }

    [RelayCommand]
    private void RemoveConnection(ConnectionViewModel c)
    {
        var action = new DelegateAction(() =>
        {
            Connections.Remove(c);
            OnConnectionRemoved(c);
        }, () =>
        {
            Connections.Add(c);
            OnConnectionAdded(c);
        }, "断开连接");
        UndoRedo.ExecuteAction(action);
    }

    [RelayCommand]
    void DeleteSelectedNodes()
    {
        if (SelectedNodes.Count == 0) return;
        var selection = SelectedNodes.ToList();
        using (UndoRedo.Batch("删除选中"))
        {
            var action = new DelegateAction(() =>
                {
                    foreach (var node in selection) Nodes.Remove(node);
                    selection.ForEach(OnNodeRemoved);
                }, () =>
                {
                    Nodes.AddRange(selection);
                    selection.ForEach(OnNodeAdded);
                },
                "删除节点");
            UndoRedo.ExecuteAction(action);
        }
    }
    
    
    protected virtual void OnNodeAdded(NodeViewModel node)
    {
        
    }

    protected virtual void OnNodeRemoved(NodeViewModel n)
    {
        foreach (var input in n.Inputs) DisconnectConnector(input);

        foreach (var output in n.Outputs) DisconnectConnector(output);

        
        void DisconnectConnector(ConnectorViewModel connector)
        {
            var connections = Connections.Where(c => c.Source == connector || c.Target == connector).ToList();
            connections.ForEach(RemoveConnection);
        }
    }
    public virtual void OnConnectionAdded(ConnectionViewModel c)
    {
        c.Source.IsConnected = true;
        c.Target.IsConnected = true;
    }

    public virtual void OnConnectionRemoved(ConnectionViewModel c)
    {
        if (Connections.All(e => (e.Source) != c.Source&& (e.Target) != c.Source))
        {
            c.Source.IsConnected = false; 
        }

        if (Connections.All(e => (e.Target != c.Target) && (e.Source != c.Target)))
        {
            c.Target.IsConnected = false;
        }
        
    }

    #endregion
    
    [ObservableProperty] MatrixTransform? _matrixTransform;


    [RelayCommand]
    void ResetView()
    {
        MatrixTransform = new MatrixTransform();
    }

    public void OnNodesMove(bool isStart)
    {
        if (isStart)
        {
            UndoRedo.ExecuteAction(new MoveShapesAction(SelectedNodes));
        }
        else
        {
            if (UndoRedo.Current is MoveShapesAction movesShapes) movesShapes.SaveLocations();
        }
    }
}

public static class NodeExtensions
{
    public static ConnectorViewModel AddInput(this NodeViewModel wrapper,string name)
    {
        var c = new ConnectorViewModel()
        {
            Header = name,
            Parent = wrapper,
            Output = false,
        };
        wrapper.Inputs.Add(c);
        return c;
    }
    public static ConnectorViewModel AddOutput(this NodeViewModel wrapper,string name)
    {
        var c = new ConnectorViewModel()
        {
            Header = name,
            Parent = wrapper,
            Output = true,
        };
        wrapper.Outputs.Add(c);
        return c;
    }
}

public partial class NodeViewModel: ObservableObject
{
    [ObservableProperty] private string title;
    [ObservableProperty] private double x, y;
    public AvaloniaList<ConnectorViewModel> Inputs { get; set; } = [];
    public AvaloniaList<ConnectorViewModel> Outputs { get; set; } = [];
    
    public NodeViewModel()
    {
        
    }
}

public partial class ConnectorViewModel: ObservableObject
{
    public NodeViewModel Parent { get; set; }
    [ObservableProperty] private string header;
    [ObservableProperty] private Point anchor;
    [ObservableProperty] private bool output;
    [ObservableProperty] private bool isConnected;
}

public partial class ConnectionViewModel: ObservableObject
{
    [ObservableProperty] private ConnectorViewModel source;
    [ObservableProperty] private ConnectorViewModel target;
}

public partial class PendingConnectionW: ConnectionViewModel
{
    [ObservableProperty] private string text;
}