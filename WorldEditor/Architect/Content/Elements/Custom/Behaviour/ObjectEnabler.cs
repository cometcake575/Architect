using System.Collections;
using System.Collections.Generic;
using Modding.Utils;
using UnityEngine;

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
        if (_toggle) StartCoroutine(_toggle.Disable(name));
    }
    
    private void Update()
    {
        if (!_setup)
        {
            _setup = true;
            var o = gameObject.scene.FindGameObject(objectPath);
            if (o)
            {
                if (o.GetComponent<Enabler>()) return;
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