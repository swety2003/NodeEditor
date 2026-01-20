using System;

namespace NodeEditorSample.UndoRedo;

public class DelegateAction : IAction
{
    private readonly Action _execute;
    private readonly Action _undo;

    public DelegateAction(Action apply, Action unapply, string? label)
    {
        _execute = apply;
        _undo = unapply;
        Label = label;
    }

    public string? Label { get; }

    public void Execute()
    {
        _execute();
    }

    public void Undo()
    {
        _undo();
    }

    public override string? ToString()
    {
        return Label;
    }
}