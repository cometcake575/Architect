using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ObjectRemover : MonoBehaviour
{
    public string triggerName;

    private Disabler[] _toggle;

    private void OnEnable()
    {
        _toggle = RoomObjects.GetObjectsDisable(this);
        foreach (var obj in _toggle) obj.Disable(name);
    }

    private void OnDisable()
    {
        foreach (var obj in _toggle) obj.Enable(name);
    }
}

public class ObjectEnabler : MonoBehaviour
{
    public string triggerName;

    private Enabler[] _toggle;

    private void OnEnable()
    {
        _toggle = RoomObjects.GetObjectsEnable(this);
        foreach (var obj in _toggle) obj.Enable(name);
    }

    private void OnDisable()
    {
        foreach (var obj in _toggle) obj.Disable(name);
    }
}

public class Disabler : MonoBehaviour
{
    public List<string> disablers = [];

    private bool _enableByDefault;

    private void OnEnable()
    {
        if (disablers.Count > 0)
        {
            _enableByDefault = true;
            gameObject.SetActive(false);
        }
    }

    public void Disable(string enableName)
    {
        disablers.Add(enableName);
        Refresh();
    }

    public void Enable(string enableName)
    {
        disablers.Remove(enableName);
        Refresh();
    }

    private void Refresh()
    {
        if (disablers.Count == 0)
        {
            if (_enableByDefault) gameObject.SetActive(true);
        }
        else if (gameObject.activeSelf)
        {
            _enableByDefault = true;
            gameObject.SetActive(false);
        }
    }
}

public class Enabler : MonoBehaviour
{
    public List<string> enablers = [];

    private bool _enableByDefault;

    private void OnEnable()
    {
        if (enablers.Count > 0)
        {
            _enableByDefault = true;
            gameObject.SetActive(true);
        }
    }

    public void Disable(string enableName)
    {
        enablers.Add(enableName);
        Refresh();
    }

    public void Enable(string enableName)
    {
        enablers.Remove(enableName);
        Refresh();
    }

    private void Refresh()
    {
        if (enablers.Count == 0)
        {
            if (_enableByDefault) gameObject.SetActive(false);
        }
        else if (gameObject.activeSelf)
        {
            _enableByDefault = true;
            gameObject.SetActive(true);
        }
    }
}

public class RoomClearerConfig : MonoBehaviour
{
    public bool removeTransitions;

    public bool removeBenches = true;

    public bool removeProps = true;

    public bool removeScenery = true;

    public bool removeTilemap = true;

    public bool removeBlur = true;

    public bool removeNpcs = true;

    public bool removeCameraLocks = true;

    public bool removeMusic = true;

    public bool removeOther = true;
}

public class ObjectEnablerConfig : MonoBehaviour
{
    public string objectPath = "";
}

public class ObjectRemoverConfig : MonoBehaviour
{
    public string objectPath = "";
}