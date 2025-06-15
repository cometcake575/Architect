using System;
using System.Globalization;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;

namespace Architect.Attributes.Config;

public class FloatConfigType : ConfigType<FloatConfigValue>
{
    public FloatConfigType(string name, Action<GameObject, FloatConfigValue> action, Func<GameObject, bool> applies = null) : base(name, action, applies) { }

    public override ConfigValue Deserialize(string data)
    {
        return new FloatConfigValue(this, Convert.ToSingle(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply)
    {
        return new FloatConfigElement(Name, root, apply);
    }
}

public class FloatConfigElement : ConfigElement
{
    private readonly TextInput _input;
    
    public FloatConfigElement(string name, LayoutRoot layout, Button apply)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80,
            ContentType = InputField.ContentType.DecimalNumber
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