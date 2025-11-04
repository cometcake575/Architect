using Architect.Attributes;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomBinder : MonoBehaviour
{
    private static AudioClip _clip;
    public string bindingType;
    public Sprite disabledSprite;
    public Sprite enabledSprite;
    public bool active = true;
    public bool reversible;
    private SpriteRenderer _renderer;
    private AudioSource _source;
    private bool _used;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = enabledSprite;
        if (!CustomObjects.Bindings.TryGetValue(bindingType, out var binders))
        {
            binders = [];
            CustomObjects.Bindings[bindingType] = binders;
        }

        binders.Add(this);

        _renderer.sprite = active ? enabledSprite : disabledSprite;
        _source = HeroController.instance.GetComponent<AudioSource>();

        CustomObjects.RefreshBinding(bindingType);
    }

    private void OnEnable()
    {
        CustomObjects.RefreshBinding(bindingType);
    }

    private void OnDisable()
    {
        CustomObjects.RefreshBinding(bindingType);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used) return;
        if (!other.gameObject.GetComponent<HeroController>()) return;

        active = !active;
        _renderer.sprite = active ? enabledSprite : disabledSprite;

        EventManager.BroadcastEvent(gameObject, active ? "OnBind" : "OnUnbind");

        if (_source)
        {
            _source.pitch = 1;
            _source.PlayOneShot(_clip, 1f);
        }

        CustomObjects.RefreshBinding(bindingType);

        if (reversible) return;
        _used = true;
        _renderer.enabled = false;
    }

    internal static void Init()
    {
        _clip = ResourceUtils.LoadInternalClip("Bindings.chain_cut");
    }
}