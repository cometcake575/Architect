using System;
using System.Globalization;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;

namespace Architect.Attributes.Config;

public class FloatConfigType(string name, Action<GameObject, FloatConfigValue> action)
    : ConfigType<FloatConfigValue>(name, action)
{
    private float? _defaultValue;

    public FloatConfigType WithDefaultValue(float value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return !_defaultValue.HasValue ? null : new FloatConfigValue(this, _defaultValue.Value);
    }
    
    public override ConfigValue Deserialize(string data)
    {
        return new FloatConfigValue(this, Convert.ToSingle(data.Replace(",", "."), CultureInfo.InvariantCulture));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new FloatConfigElement(Name, root, apply, oldValue);
    }
}

public class FloatConfigElement : ConfigElement
{
    private readonly TextInput _input;

    public FloatConfigElement(string name, LayoutRoot layout, Button apply, [CanBeNull] string oldValue)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80,
            ContentType = InputField.ContentType.DecimalNumber
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

public class FloatConfigValue : ConfigValue<FloatConfigType>
{
    public FloatConfigValue(FloatConfigType type, float value) : base(type)
    {
        _value = value;
    }

    private readonly float _value;

    public float GetValue()
    {
        return _value;
    }

    public override string SerializeValue()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }
}