using System;
using JetBrains.Annotations;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;

namespace Architect.Attributes.Config;

public class ChoiceConfigType : ConfigType<ChoiceConfigValue>
{
    private readonly string[] _options;

    public ChoiceConfigType(string name, Action<GameObject, ChoiceConfigValue> action, bool preAwake = false, params string[] options) :
        base(name, action, preAwake)
    {
        _options = options;
    }

    public override ConfigValue Deserialize(string data)
    {
        return new ChoiceConfigValue(this, Convert.ToInt32(data));
    }

    public override ConfigElement CreateInput(LayoutRoot root, Button apply, string oldValue)
    {
        return new ChoiceConfigElement(Name, root, apply, _options, oldValue);
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

        _choice = oldValue != null ? Array.IndexOf(options, oldValue) : -1;

        _input.Content = oldValue ?? "Unset";

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