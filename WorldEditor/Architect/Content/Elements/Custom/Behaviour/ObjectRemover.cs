using System.Collections.Generic;
using UnityEngine;
using Architect.Utils;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ObjectEnabler : MonoBehaviour
{
    public string objectPath;
    
    private Enabler _toggle;

    private bool _shouldEnable;
    private bool _setup;
        
    private void OnEnable()
    {
        _shouldEnable = true;
    }

    private void OnDisable()
    {
        if (_toggle) ArchitectPlugin.Instance.StartCoroutine(_toggle.Disable(name));
    }
    
    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            var o = ObjectUtils.GetGameObjectFromArray(gameObject.scene.GetRootGameObjects(), objectPath);
            if (o)
            {
                if (o.GetComponent<Disabler>()) return;
                _toggle = o.GetOrAddComponent<Enabler>();
            }
        }
        
        if (_shouldEnable)
        {
            _shouldEnable = false;
            DoEnable();
        }
    }

    private void DoEnable()
    {
        if (_toggle) _toggle.Enable(name);
    }
}

public class Enabler : MonoBehaviour
{
    public List<string> enablers = [];

    private bool _disableByDefault;

    private void OnDisable()
    {
        if (enablers.Count > 0)
        {
            _disableByDefault = true;
            gameObject.SetActive(true);
        }
    }

    public void Enable(string enableName)
    {
        enablers.Add(enableName);
        Refresh();
    }

    public IEnumerator Disable(string enableName)
    {
        yield return null;

        if (!this) yield break;
        
        enablers.Remove(enableName);
        Refresh();
    }

    private void Refresh()
    {
        if (enablers.Count == 0)
        {
            if (_disableByDefault) gameObject.SetActive(false);
        }
        else if (!gameObject.activeSelf)
        {
            _disableByDefault = true;
            gameObject.SetActive(true);
        }
    }
}

public class ObjectRemover : MonoBehaviour
{
    public string triggerName;

    private Disabler[] _toggle;

    private void OnEnable()
    {
        _toggle = RoomObjects.GetObjects(this);
        foreach (var obj in _toggle) obj.Disable(name);
    }

    private void OnDisable()
    {
        foreach (var obj in _toggle) obj.Enable(name);
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

public class ObjectRemoverConfig : MonoBehaviour
{
    public string objectPath = "";
}