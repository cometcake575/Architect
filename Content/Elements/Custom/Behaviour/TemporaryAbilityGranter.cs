using System.Collections;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class TemporaryAbilityGranter : MonoBehaviour
{
    private static readonly Sprite Used = ResourceUtils.LoadInternal("used_crystal", FilterMode.Point);
    private static readonly AudioClip Use = ResourceUtils.LoadInternalClip("crystal_use");
    private static readonly AudioClip Return = ResourceUtils.LoadInternalClip("crystal_return");
    public float disableTime = 2.5f;

    public string abilityType;
    public bool singleUse;
    private bool _disabled;

    private SpriteRenderer _renderer;
    private AudioSource _source;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _source = HeroController.instance.GetComponent<AudioSource>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;

        if (_disabled) return;
        CustomObjects.TemporaryAbilities.Add(abilityType);
        if (singleUse)
        {
            Destroy(gameObject);
        }
        else
        {
            _disabled = true;
            StartCoroutine(TempDisable());
        }

        CustomObjects.CollectAbilityGranter(abilityType);
        _source.pitch = 1;
        _source.PlayOneShot(Use);
    }

    private IEnumerator TempDisable()
    {
        var sprite = _renderer.sprite;
        _renderer.sprite = Used;
        yield return new WaitForSeconds(disableTime);
        _renderer.sprite = sprite;
        _disabled = false;
        _source.pitch = 1;
        _source.PlayOneShot(Return);
    }
}