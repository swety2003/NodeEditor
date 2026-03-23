# NodeEditor 用户指南

## 概述

NodeEditor 是一个 Avalonia UI 控件库，用于创建可视化节点编辑器。它允许用户通过拖拽和连接节点来构建流程图或数据流图。

## 架构

NodeEditor 主要由以下组件组成：

- **SampleNodeEditor**: 主控件，承载节点和连接。
- **Node**: 表示单个节点，包含输入和输出端口。
- **Connection**: 表示节点间的连接线。
- **Connector**: 节点的端口，用于连接。
- **CanvasItemsControl**: 管理画布上的项。
- **ZoomBorder**: 支持缩放和平移。

## 数据模型

### NodeViewModel

表示一个节点：

```csharp
public class NodeViewModel
{
    public string Title { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public AvaloniaList<ConnectorViewModel> Inputs { get; set; }
    public AvaloniaList<ConnectorViewModel> Outputs { get; set; }
}
```

### ConnectorViewModel

表示节点的端口：

```csharp
public class ConnectorViewModel
{
    public string Title { get; set; }
    public Point Position { get; set; }
}
```

### ConnectionViewModel

表示连接：

```csharp
public class ConnectionViewModel
{
    public ConnectorViewModel Source { get; set; }
    public ConnectorViewModel Target { get; set; }
}
```

## 集成步骤

### 1. 添加依赖

在你的 `.csproj` 文件中添加：

```xml
<ItemGroup>
    <PackageReference Include="MyControls.NodeEditor" Version="1.0.0" />
</ItemGroup>
```

### 2. 配置 XAML

在你的用户控件或窗口中：

```xml
<UserControl xmlns:controls="clr-namespace:NodeEditor.Controls;assembly=NodeEditor">
    <UserControl.Styles>
        <StyleInclude Source="avares://NodeEditor/Controls/Connector.axaml" />
        <StyleInclude Source="avares://NodeEditor/Controls/Node.axaml" />
        <StyleInclude Source="avares://NodeEditor/Controls/SampleNodeEditor.axaml" />
    </UserControl.Styles>
    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="avares://NodeEditor/Themes/CanvasItem.axaml" />
                <ResourceInclude Source="avares://NodeEditor/Themes/CanvasItemsControl.axaml" />
                <ResourceInclude Source="avares://NodeEditor/Themes/ConnectorsControl.axaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>
    <controls:SampleNodeEditor
        Connections="{Binding Connections}"
        EditorOptions="{Binding Options}"
        Nodes="{Binding Nodes}"
        SelectedNodes="{Binding SelectedNodes}"
        ViewTransform="{Binding ViewTransform}" />
</UserControl>
```

### 3. 设置 ViewModel

```csharp
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Controls;

public partial class NodeEditorViewModel : ObservableObject
{
    public AvaloniaList<NodeViewModel> Nodes { get; } = [];
    public AvaloniaList<ConnectionViewModel> Connections { get; } = [];
    public AvaloniaList<NodeViewModel> SelectedNodes { get; } = [];
    public PendingConnection PendingConnection { get; } = new();
    public EditorOptions Options { get; set; } = EditorOptions.All;
    public MatrixTransform ViewTransform { get; set; } = new MatrixTransform();

    public NodeEditorViewModel()
    {
        // 初始化一些示例节点
        var node1 = new NodeViewModel { Title = "Start", X = 100, Y = 100 };
        node1.AddInput("Input");
        node1.AddOutput("Output");

        var node2 = new NodeViewModel { Title = "Process", X = 300, Y = 100 };
        node2.AddInput("Input");
        node2.AddOutput("Output");

        Nodes.Add(node1);
        Nodes.Add(node2);
    }
}
```

## 编辑器选项

`EditorOptions` 是一个标志枚举，用于控制编辑器的行为：

- `DragMove`: 允许拖拽移动节点
- `Connection`: 允许创建连接
- `All`: 启用所有功能

## 事件和命令

NodeEditor 支持以下命令：

- `ResetViewCommand`: 重置视图缩放和平移
- `NewNodeCommand`: 添加新节点
- `UndoCommand`: 撤销
- `RedoCommand`: 重做

## 自定义样式

你可以自定义节点和连接的外观，通过修改主题文件或覆盖样式。

## 撤销/重做

NodeEditor 支持撤销/重做操作。使用 `ActionsHistory` 类来管理操作历史。

```csharp
using NodeEditorSample.UndoRedo;

public class MyViewModel
{
    private ActionsHistory UndoRedo { get; } = new();

    [RelayCommand]
    void Undo() => UndoRedo.Undo();

    [RelayCommand]
    void Redo() => UndoRedo.Redo();
}
```

## 示例项目

参考 `sample/NodeEditorSample` 项目，该项目展示了完整的集成示例，包括：

- 节点创建和删除
- 连接创建和删除
- 视图操作
- 撤销/重做

## 故障排除

### 常见问题

1. **控件不显示**: 确保所有必要的样式和资源已包含。
2. **连接不工作**: 检查 `EditorOptions` 是否启用了 `Connection`。
3. **节点不移动**: 检查 `EditorOptions` 是否启用了 `DragMove`。

### 调试

启用 Avalonia 的日志记录以查看调试信息：

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();
```

## API 参考

详细的 API 文档请参考源码中的 XML 注释。

## 贡献

如果你发现问题或有改进建议，请提交 Issue 或 Pull Request。