using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom;

public class CustomDamager : MonoBehaviour
{
    public int damageAmount;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.gameObject.GetComponent<HeroController>();
        if (!controller) return;
        controller.TakeDamage(
            gameObject,
            CollisionSide.other,
            damageAmount,
            2
        );
    }
}