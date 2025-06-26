using System;
using System.Collections.Generic;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content.Elements.Custom;

public class CustomBinder : MonoBehaviour
{
    public string bindingType;
    private SpriteRenderer _renderer;
    private AudioSource _source;
    public Sprite disabledSprite;
    public Sprite enabledSprite;
    public bool active = true;
    public bool reversible;
    private bool _used;

    public AudioClip clip;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = enabledSprite;
        if (!CustomObjects.Bindings.TryGetValue(bindingType, out var binders))
        {
            binders = new List<CustomBinder>();
            CustomObjects.Bindings[bindingType] = binders;
        }
        binders.Add(this);

        _renderer.sprite = active ? enabledSprite : disabledSprite;
        _source = HeroController.instance.GetComponent<AudioSource>();
        
        CustomObjects.RefreshBinding(bindingType);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used) return;
        if (!other.gameObject.GetComponent<HeroController>()) return;
        
        active = !active;
        _renderer.sprite = active ? enabledSprite : disabledSprite;
        
        EventManager.BroadcastEvent(gameObject, active ? EventBroadcasterType.OnBind : EventBroadcasterType.OnUnbind);
        
        if (_source) _source.PlayOneShot(clip, 1f);
        
        CustomObjects.RefreshBinding(bindingType);

        if (reversible) return;
        _used = true;
        _renderer.enabled = false;
    }
}