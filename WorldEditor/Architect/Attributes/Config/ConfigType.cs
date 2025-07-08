using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public abstract class ConfigType
{
    public readonly string Name;
    public readonly bool PreAwake;
    
    protected ConfigType(string name, bool preAwake)
    {
        Name = name;
        PreAwake = preAwake;
    }
    
    public abstract ConfigValue Deserialize(string data);

    public abstract ConfigElement CreateInput(LayoutRoot root, Button apply, [CanBeNull] string oldValue);
    
    internal abstract void RunAction(GameObject obj, ConfigValue value);
}

public abstract class ConfigType<TValue> : ConfigType where TValue : ConfigValue
{
    private readonly Action<GameObject, TValue> _action;

    protected ConfigType(string name, Action<GameObject, TValue> action, bool preAwake) : base(name, preAwake)
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

    public abstract bool PreAwake();
    
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

    public override bool PreAwake()
    {
        return _type.PreAwake;
    }
}

public abstract class ConfigElement
{
    public abstract ArrangableElement GetElement();

    public abstract string GetValue();
}