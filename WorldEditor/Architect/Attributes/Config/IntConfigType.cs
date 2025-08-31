using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;

namespace Architect.Attributes.Config;

public class IntConfigType(string name, Action<GameObject, IntConfigValue> action)
    : ConfigType<IntConfigValue>(name, action)
{
    private int? _defaultValue;

    public IntConfigType WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return !_defaultValue.HasValue ? null : new IntConfigValue(this, _defaultValue.Value);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new IntConfigValue(this, Convert.ToInt32(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new IntConfigElement(Name, root, apply, oldValue);
    }
}

public class IntConfigElement : ConfigElement
{
    private readonly TextInput _input;

    public IntConfigElement(string name, LayoutRoot layout, Button apply, [CanBeNull] string oldValue)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80,
            ContentType = InputField.ContentType.IntegerNumber
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

public class IntConfigValue : ConfigValue<IntConfigType>
{
    private readonly int _value;

    public IntConfigValue(IntConfigType type, int value) : base(type)
    {
        _value = value;
    }

    public int GetValue()
    {
        return _value;
    }

    public override string SerializeValue()
    {
        return _value.ToString();
    }
}