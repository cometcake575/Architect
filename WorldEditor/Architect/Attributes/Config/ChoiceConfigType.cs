using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public class ChoiceConfigType(string name, Action<GameObject, ChoiceConfigValue> action, params string[] options)
    : ConfigType<ChoiceConfigValue>(name, action)
{
    private int? _defaultValue;

    public ChoiceConfigType WithDefaultValue(int value)
    {
        _defaultValue = value;
        return this;
    }

    public override ConfigValue GetDefaultValue()
    {
        return !_defaultValue.HasValue ? null : new ChoiceConfigValue(this, _defaultValue.Value);
    }

    public override ConfigValue Deserialize(string data)
    {
        return new ChoiceConfigValue(this, Convert.ToInt32(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new ChoiceConfigElement(Name, root, apply, options, oldValue);
    }
}

public class ChoiceConfigElement : ConfigElement
{
    private readonly Button _input;
    private int _choice;
    
    public ChoiceConfigElement(string name, LayoutRoot layout, Button apply, string[] options, [CanBeNull] string oldValue)
    {
        _input = new Button(layout, name + " Input")
        {
            MinWidth = 20
        };

        if (oldValue != null)
        {
            var oldIndex = Convert.ToInt32(oldValue);
            _choice = oldIndex;
            _input.Content = options[oldIndex];
        }
        else
        {
            _choice = -1;
            _input.Content = "Default";
        }

        _input.Click += button =>
        {
            _choice = (_choice + 1) % options.Length;
            button.Content = options[_choice];
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

public class ChoiceConfigValue : ConfigValue<ChoiceConfigType>
{
    public ChoiceConfigValue(ChoiceConfigType type, int value) : base(type)
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