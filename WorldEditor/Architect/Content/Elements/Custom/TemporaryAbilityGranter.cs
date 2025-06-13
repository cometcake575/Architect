using UnityEngine;

namespace Architect.Content.Elements.Custom;

public class TemporaryAbilityGranter : MonoBehaviour
{
    public string abilityType;
    public bool singleUse;
    private SpriteRenderer _renderer;
    private float _disabled;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;
        
        if (_disabled > 0) return;
        CustomObjects.TemporaryAbilities.Add(abilityType);
        if (singleUse) Destroy(gameObject);
        else
        {
            _renderer.enabled = false;
            _disabled = 2.5f;
        }
    }

    private void Update()
    {
        if (_disabled <= 0) return;
        _disabled -= Time.deltaTime;
        if (_disabled > 0) return;
        _renderer.enabled = true;
    }
}