using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomDamager : MonoBehaviour
{
    public int damageAmount;
    public int damageType = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.gameObject.GetComponent<HeroController>();
        if (!controller) return;
        controller.TakeDamage(
            gameObject,
            CollisionSide.other,
            damageAmount,
            damageType
        );
    }
}