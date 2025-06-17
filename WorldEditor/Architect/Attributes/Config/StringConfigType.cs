using System;
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

    public override ConfigElement CreateInput(LayoutRoot root, Button apply)
    {
        return new StringConfigElement(Name, root, apply);
    }
}

public class StringConfigElement : ConfigElement
{
    private readonly TextInput _input;
    
    public StringConfigElement(string name, LayoutRoot layout, Button apply)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80
        };

        _input.TextChanged += (_, _) =>
        {
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