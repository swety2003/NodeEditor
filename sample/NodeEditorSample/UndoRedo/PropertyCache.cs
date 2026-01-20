using System;
using System.Collections.Generic;
using System.Reflection;

namespace NodeEditorSample.UndoRedo;

public interface IPropertyAccessor
{
    bool CanRead { get; }
    bool CanWrite { get; }
    object? GetValue(object instance);
    void SetValue(object instance, object? value);
}

public sealed class PropertyAccessor<TInstanceType, TPropertyType> : IPropertyAccessor where TInstanceType : class
{
    private readonly Func<TInstanceType, TPropertyType> _getter;
    private readonly Action<TInstanceType, TPropertyType> _setter;

    public PropertyAccessor(Func<TInstanceType, TPropertyType> getter, Action<TInstanceType, TPropertyType> setter)
    {
        _getter = getter;
        _setter = setter;

        CanRead = getter != null;
        CanWrite = setter != null;
    }

    public bool CanRead { get; }
    public bool CanWrite { get; }

    public object? GetValue(object instance)
    {
        return _getter((TInstanceType)instance);
    }

    public void SetValue(object instance, object? value)
    {
        _setter((TInstanceType)instance, (TPropertyType)value!);
    }
}

public class PropertyCache
{
    private static readonly Dictionary<string, IPropertyAccessor> _properties = new();

    public static IPropertyAccessor Get(Type type, string name)
    {
        var propKey = $"{type.FullName}.{name}";
        if (!_properties.TryGetValue(propKey, out var result))
        {
            var prop = type.GetProperty(name);
            result = Create(type, prop!);

            _properties.Add(propKey, result);
        }

        return result;
    }

    public static IPropertyAccessor Get<T>(string name)
    {
        return Get(typeof(T), name);
    }

    private static IPropertyAccessor Create(Type type, PropertyInfo property)
    {
        Delegate? getterInvocation = default;
        Delegate? setterInvocation = default;

        if (property.CanRead)
        {
            var getMethod = property.GetGetMethod(true)!;
            var getterType = typeof(Func<,>).MakeGenericType(type, property.PropertyType);
            getterInvocation = Delegate.CreateDelegate(getterType, getMethod);
        }

        if (property.CanWrite)
        {
            var setMethod = property.GetSetMethod(true)!;
            var setterType = typeof(Action<,>).MakeGenericType(type, property.PropertyType);
            setterInvocation = Delegate.CreateDelegate(setterType, setMethod);
        }

        var adapterType = typeof(PropertyAccessor<,>).MakeGenericType(type, property.PropertyType);

        return (IPropertyAccessor)Activator.CreateInstance(adapterType, getterInvocation, setterInvocation)!;
    }
}