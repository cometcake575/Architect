using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public abstract class ConfigType
{
    public readonly string Name;
    private Func<GameObject, bool> _condition;
    public string Id;

    public bool IsPreAwake;

    protected ConfigType(string name)
    {
        Name = name;
    }

    public bool Check(GameObject obj)
    {
        return _condition == null || _condition.Invoke(obj);
    }

    public abstract ConfigValue Deserialize(string data);

    [CanBeNull]
    public abstract ConfigValue GetDefaultValue();

    public abstract ConfigElement CreateInput(LayoutRoot root, Button apply, [CanBeNull] string oldValue);

    public ConfigType WithCondition(Func<GameObject, bool> condition)
    {
        _condition = condition;
        return this;
    }

    internal abstract void RunAction(GameObject obj, ConfigValue value);
}

public abstract class ConfigType<TValue> : ConfigType where TValue : ConfigValue
{
    private readonly Action<GameObject, TValue> _action;

    protected ConfigType(string name, Action<GameObject, TValue> action) : base(name)
    {
        _action = action;
    }

    public ConfigType<TValue> PreAwake()
    {
        IsPreAwake = true;
        return this;
    }

    internal override void RunAction(GameObject obj, ConfigValue value)
    {
        try
        {
            _action.Invoke(obj, value as TValue);
        }
        catch (Exception e)
        {
            Architect.Instance.LogError("Error attempting to apply config \"" + value.GetTypeId() + "\"");
            Architect.Instance.LogError(e.Message);
            Architect.Instance.LogError(e.StackTrace);
        }
    }
}

public abstract class ConfigValue
{
    public abstract string SerializeValue();

    public abstract string GetTypeId();

    public abstract string GetName();

    public abstract bool PreAwake();

    public abstract void Setup(GameObject obj);
}

public abstract class ConfigValue<TType> : ConfigValue where TType : ConfigType
{
    private readonly TType _type;

    protected ConfigValue(TType type)
    {
        _type = type;
    }

    public override string GetTypeId()
    {
        return _type.Id;
    }

    public override string GetName()
    {
        return _type.Name;
    }

    public override void Setup(GameObject obj)
    {
        _type.RunAction(obj, this);
    }

    public override bool PreAwake()
    {
        return _type.IsPreAwake;
    }
}

public abstract class ConfigElement
{
    public abstract ArrangableElement GetElement();

    public abstract string GetValue();
}