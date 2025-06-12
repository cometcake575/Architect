using System;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Button = MagicUI.Elements.Button;

namespace Architect.Attributes.Config;

public class IntConfigType : ConfigType<IntConfigValue>
{
    public IntConfigType(string name, Action<GameObject, IntConfigValue> action, Func<GameObject, bool> applies = null) : base(name, action, applies) { }

    public override ConfigValue Deserialize(string data)
    {
        return new IntConfigValue(this, Convert.ToInt32(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply)
    {
        return new IntConfigElement(Name, root, apply);
    }
}

public class IntConfigElement : ConfigElement
{
    private readonly TextInput _input;
    
    public IntConfigElement(string name, LayoutRoot layout, Button apply)
    {
        _input = new TextInput(layout, name + " Input")
        {
            MinWidth = 80,
            ContentType = InputField.ContentType.IntegerNumber
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

public class IntConfigValue : ConfigValue<IntConfigType>
{
    public IntConfigValue(IntConfigType type, int value) : base(type)
    {
        _value = value;
    }

    private readonly int _value;

    public int GetValue()
    {
        return _value;
    }

    public override string SerializeValue()
    {
        return _value.ToString();
    }
}