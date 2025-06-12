using System;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public abstract class ConfigType
{
    public readonly string Name;
    private readonly Func<GameObject, bool> _applies;
    
    protected ConfigType(string name, Func<GameObject, bool> applies)
    {
        Name = name;
        _applies = applies;
    }

    public bool DoesApply(GameObject obj)
    {
        return _applies == null || _applies.Invoke(obj);
    }
    
    public abstract ConfigValue Deserialize(string data);

    public abstract ConfigElement CreateInput(LayoutRoot root, Button apply);
    
    internal abstract void RunAction(GameObject obj, ConfigValue value);
}

public abstract class ConfigType<TValue> : ConfigType where TValue : ConfigValue
{
    private readonly Action<GameObject, TValue> _action;

    protected ConfigType(string name, Action<GameObject, TValue> action, Func<GameObject, bool> applies) : base(name, applies)
    {
        _action = action;
    }

    internal override void RunAction(GameObject obj, ConfigValue value)
    {
        _action.Invoke(obj, value as TValue);
    }
}

public abstract class ConfigValue
{
    public abstract string SerializeValue();

    public abstract string GetName();
    
    public abstract void Setup(GameObject obj);
}

public abstract class ConfigValue<TType> : ConfigValue where TType : ConfigType
{
    protected ConfigValue(TType type)
    {
        _type = type;
    }

    public override string GetName()
    {
        return _type.Name;
    }

    private readonly TType _type;

    public override void Setup(GameObject obj)
    {
        _type.RunAction(obj, this);
    }
}

public abstract class ConfigElement
{
    public abstract ArrangableElement GetElement();

    public abstract string GetValue();
}