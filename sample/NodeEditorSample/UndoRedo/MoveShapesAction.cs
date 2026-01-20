using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using NodeEditorSample.ViewModels;

namespace NodeEditorSample.UndoRedo;

public class MoveShapesAction : IAction
{
    private readonly IReadOnlyCollection<NodeViewModel> _movedShapes;
    private Dictionary<NodeViewModel, Point>? _finalLocations;
    private Dictionary<NodeViewModel, Point> _initialLocations;
    private bool err;

    public MoveShapesAction(IEnumerable<NodeViewModel> _movedShapes)
    {
        try
        {
            this._movedShapes = _movedShapes.ToList();
            InitLocations();
        }
        catch (Exception e)
        {
            err = true;
            Debugger.Break();
        }
    }

    public string? Label { get; set; } = "移动对象";

    public void Execute()
    {
        if (err || _finalLocations is null) return;
        foreach (var x in _finalLocations)
        {
            x.Key.X = x.Value.X;
            x.Key.Y = x.Value.Y;
        }
    }

    public void Undo()
    {
        if (err || _initialLocations is null) return;
        foreach (var x in _initialLocations)
        {
            x.Key.X = x.Value.X;
            x.Key.Y = x.Value.Y;
        }
    }

    private void InitLocations()
    {
        _initialLocations = _movedShapes.ToDictionary<NodeViewModel, NodeViewModel, Point>(x => x, x => new Point(x.X,x.Y));
    }

    public void SaveLocations()
    {
        try
        {
            _finalLocations = _movedShapes.ToDictionary<NodeViewModel, NodeViewModel, Point>(x => x, x => new Point(x.X,x.Y));
        }
        catch (Exception e)
        {
            err = true;
            Debugger.Break();
        }
    }
}