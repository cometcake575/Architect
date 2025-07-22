using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public class BoolConfigType(string name, Action<GameObject, BoolConfigValue> action)
    : ConfigType<BoolConfigValue>(name, action)
{
    private bool? _defaultValue;

    public BoolConfigType WithDefaultValue(bool value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return !_defaultValue.HasValue ? null : new BoolConfigValue(this, _defaultValue.Value);
    }
    
    public override ConfigValue Deserialize(string data)
    {
        return new BoolConfigValue(this, Convert.ToBoolean(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new BoolConfigElement(Name, root, apply, oldValue);
    }
}

public class BoolConfigElement : ConfigElement
{
    private readonly Button _input;
    private bool _choice;
    
    public BoolConfigElement(string name, LayoutRoot layout, Button apply, [CanBeNull] string oldValue)
    {
        _input = new Button(layout, name + " Input")
        {
            MinWidth = 20
        };
        _choice = true;

        if (oldValue != null)
        {
            _choice = oldValue == bool.TrueString;
            _input.Content = _choice.ToString();
        } else _input.Content = "Unset";

        _input.Click += button =>
        {
            _choice = !_choice;
            button.Content = _choice.ToString();
            apply.Enabled = true;
        };
    }

    public override ArrangableElement GetElement()
    {
        return _input;
    }

    public override string GetValue()
    {
        return _choice.ToString();
    }
}

public class BoolConfigValue : ConfigValue<BoolConfigType>
{
    public BoolConfigValue(BoolConfigType type, bool value) : base(type)
    {
        _value = value;
    }

    private readonly bool _value;

    public bool GetValue()
    {
        return _value;
    }

    public override string SerializeValue()
    {
        return _value.ToString();
    }
}