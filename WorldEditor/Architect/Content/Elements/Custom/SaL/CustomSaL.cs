using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom.SaL;

public class CustomSaL
{
    private static GameObject MakeObject(string name, string sprite)
    {
        var obj = new GameObject { name = name };

        obj.AddComponent<SpriteRenderer>().sprite = ResourceUtils.Load(sprite, FilterMode.Point, ppu:10);
        obj.SetActive(false);

        Object.DontDestroyOnLoad(obj);
        
        return obj;
    }

    public static AbstractPackElement MakeCrystalObject()
    {
        var obj = MakeObject("Celeste Crystal", "ScatteredAndLost.crystal");

        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        obj.AddComponent<CustomDamager>().damageAmount = 1;

        return new SimplePackElement(obj, "Celeste Crystal", "Scattered & Lost")
            .WithConfigGroup(ConfigGroup.Colours);
    }
}