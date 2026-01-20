using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Expression = System.Linq.Expressions.Expression;

namespace NodeEditorSample.UndoRedo;

[Flags]
public enum PropertyFlags
{
    Disable = 0,
    Enable = 1
}

public abstract class Undoable : ObservableObjectN
{
    private readonly HashSet<string> _trackedProperties = new();

    public Undoable()
    {
        History = ActionsHistory.Global;
    }

    public Undoable(IActionsHistory history)
    {
        History = history;
    }

    public IActionsHistory History { get; }

    private void RecordHistory<TPropType>(string propName, TPropType previous, TPropType current)
    {
        if (_trackedProperties.Contains(propName))
        {
            var prop = PropertyCache.Get(GetType(), propName);
            History.Record(() => prop.SetValue(this, current), () => prop.SetValue(this, previous), propName);
        }
    }

    protected void RecordProperty(string propName, PropertyFlags flags = PropertyFlags.Enable)
    {
        if (flags == PropertyFlags.Disable)
            _trackedProperties.Remove(propName);
        else if (flags.HasFlag(PropertyFlags.Enable)) _trackedProperties.Add(propName);
    }

    protected void RecordProperty<TType>(Expression<Func<TType, object?>> selector,
        PropertyFlags flags = PropertyFlags.Enable)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
            return;

        var name = GetPropertyName(selector);
        RecordProperty(name, flags);
    }

    private static string GetPropertyName(Expression memberAccess)
    {
        return memberAccess switch
        {
            LambdaExpression lambda => GetPropertyName(lambda.Body),
            MemberExpression mbr => mbr.Member.Name,
            UnaryExpression unary => GetPropertyName(unary.Operand),
            _ => throw new Exception($"Member name could not be extracted from {memberAccess}.")
        };
    }

    protected override bool SetProperty<TPropType>(ref TPropType field, TPropType value,
        [CallerMemberName] string propertyName = "")
    {
        var prev = field;
        if (base.SetProperty(ref field, value, propertyName))
        {
            RecordHistory(propertyName, prev, value);
            return true;
        }

        return false;
    }
}

public class ObservableObjectN : INotifyPropertyChanged
{
    /// <summary>
    ///     Gets or sets the dispatcher to use to dispatch PropertyChanged events. Defaults to UI thread.
    /// </summary>
    public virtual Action<Action> PropertyChangedDispatcher { get; set; } = Dispatcher.UIThread.Invoke;

    /// <summary>
    ///     Occurs when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Fires the PropertyChanged notification.
    /// </summary>
    /// <remarks>Specially named so that Fody.PropertyChanged calls it</remarks>
    /// <param name="propertyName">Name of the property to raise the notification for</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChangedDispatcher(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    /// <summary>
    ///     Takes, by reference, a field, and its new value. If field != value, will set field = value and raise a
    ///     PropertyChanged notification
    /// </summary>
    /// <typeparam name="T">Type of field being set and notified</typeparam>
    /// <param name="field">Field to assign</param>
    /// <param name="value">Value to assign to the field, if it differs</param>
    /// <param name="propertyName">Name of the property to notify for. Defaults to the calling property</param>
    /// <returns>True if field != value and a notification was raised; false otherwise</returns>
    protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        return false;
    }
}