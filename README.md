# NodeEditor

NodeEditor 是一个基于 Avalonia UI 的节点编辑器控件库，允许开发者轻松创建可视化节点编辑界面，支持节点拖拽、连接、缩放等功能。

## 特性

- 基于 Avalonia UI，支持跨平台（Windows, macOS, Linux）
- 支持节点拖拽和移动
- 支持节点间连接
- 支持缩放和平移视图
- 支持撤销/重做操作
- 高度可定制

## 安装

### NuGet 包 （暂不可用）

```bash
dotnet add package MyControls.NodeEditor
```

### 从源码构建

1. 克隆仓库：
   ```bash
   git clone https://github.com/swety2003/NodeEditor.git
   cd NodeEditor
   ```

2. 构建项目：
   ```bash
   dotnet build
   ```

3. 在你的项目中引用 `src/NodeEditor/NodeEditor.csproj`。

## 使用

### 基本设置

1. 在你的 Avalonia 项目中，添加对 NodeEditor 的引用。

2. 在 XAML 中包含必要的样式和资源：

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
</UserControl>
```

3. 在你的 ViewModel 中，定义节点和连接的数据：

```csharp
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Controls;

public partial class MyViewModel : ObservableObject
{
    public AvaloniaList<NodeViewModel> Nodes { get; set; } = [];
    public AvaloniaList<ConnectionViewModel> Connections { get; set; } = [];
    public AvaloniaList<NodeViewModel> SelectedNodes { get; set; } = [];
    public PendingConnection PendingConnection { get; set; } = new();
    public EditorOptions Options { get; set; } = EditorOptions.All;
    public MatrixTransform ViewTransform { get; set; } = new MatrixTransform();
}
```

4. 在 XAML 中使用 SampleNodeEditor 控件：

```xml
<controls:SampleNodeEditor
    Connections="{Binding Connections}"
    EditorOptions="{Binding Options}"
    Nodes="{Binding Nodes}"
    SelectedNodes="{Binding SelectedNodes}"
    ViewTransform="{Binding ViewTransform}" />
```

### 示例

参考 `sample/NodeEditorSample` 项目，该示例展示了如何集成 NodeEditor 并实现基本的节点编辑功能，包括添加节点、连接节点和撤销/重做。

运行示例：
```bash
cd sample/NodeEditorSample
dotnet run
```

## 文档

详细的使用文档请参考 [doc/UserGuide.md](doc/UserGuide.md)。

## 贡献

欢迎贡献！请提交 Issue 或 Pull Request。

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。