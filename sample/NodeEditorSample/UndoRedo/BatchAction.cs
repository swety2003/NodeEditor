using System.Collections.Generic;
using System.Linq;

namespace NodeEditorSample.UndoRedo;

public class BatchAction : IAction
{
    public BatchAction(string? label, IEnumerable<IAction> history)
    {
        History = history.Reverse().ToList();
        Label = label;
    }

    public IReadOnlyList<IAction> History { get; }

    public string? Label { get; }

    public void Execute()
    {
        for (var i = History.Count - 1; i >= 0; i--) History[i].Execute();
    }

    public void Undo()
    {
        for (var i = 0; i < History.Count; i++) History[i].Undo();
    }

    public override string? ToString()
    {
        return Label;
    }
}