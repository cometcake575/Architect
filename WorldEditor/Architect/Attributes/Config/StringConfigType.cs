using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public class StringConfigType : ConfigType<StringConfigValue>
{
    public StringConfigType(string name, Action<GameObject, StringConfigValue> action, bool preAwake = false) : base(name, action, preAwake) { }

    public override ConfigValue Deserialize(string data)
    {
        return new StringConfigValue(this, data);
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new StringConfigElement(Name, root, apply, oldValue);
    }
}

public class StringConfigElement : ConfigElement
{
    private readonly TextInput _input;
    
    public StringConfigElement(string name, LayoutRoot layout, Button apply, [CanBeNull] string oldValue)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80
        };
        
        if (oldValue != null) _input.Text = oldValue;
        var last = _input.Text;

        _input.TextChanged += (_, n) =>
        {
            if (last == n) return;
            last = n;
            apply.Enabled = true;
        };
    }

    public override ArrangableElement GetElement()
    {
        return _input;
    }

    public override string GetValue()
    {
        return _input.Text;
    }
}

public class StringConfigValue : ConfigValue<StringConfigType>
{
    public StringConfigValue(StringConfigType type, string value) : base(type)
    {
        _value = value;
    }

    private readonly string _value;

    public string GetValue()
    {
        return _value;
    }

    public override string SerializeValue()
    {
        return _value;
    }
}