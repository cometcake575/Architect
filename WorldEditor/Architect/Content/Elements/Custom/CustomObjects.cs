using System.Collections.Generic;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom;

public static class CustomObjects
{
    internal static readonly HashSet<string> TemporaryAbilities = new();
    
    public static void Initialize()
    {
        var customs = new ContentPack("Custom", "Custom assets that don't exist in the base game")
        {
            new SimplePackElement(CreateHazardRespawnPoint(), "Hazard Respawn Point", "Custom", WeSpriteUtils.Load("hazard_respawn_point")),
            new SimplePackElement(CreateZoteTrophy(), "Winner's Trophy", "Custom"),
            CreateTemporaryAbilityGranter("dash_crystal", "Dash", false, "Dash Crystal"),
            CreateTemporaryAbilityGranter("single_dash_crystal", "Dash", true, "Single Use Dash Crystal"),
            CreateTemporaryAbilityGranter("wings_crystal", "Wings", false, "Wings Crystal"),
            CreateTemporaryAbilityGranter("single_wings_crystal", "Wings", true, "Single Use Wings Crystal")
        };
        ContentPacks.RegisterPack(customs);

        InitializeAbilityGranterHooks();
    }

    private static GameObject CreateHazardRespawnPoint()
    {
        var point = new GameObject("Hazard Respawn Point");

        point.AddComponent<HazardRespawnTrigger>().respawnMarker = point.AddComponent<CustomHazardRespawnMarker>();

        var collider = point.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        
        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateZoteTrophy()
    {
        var sprite = WeSpriteUtils.Load("zote_trophy");
        var obj = new GameObject("Winner's Trophy");
        
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        obj.AddComponent<ParticleSystem>();

        obj.AddComponent<ZoteTrophy>();
        
        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);

        return obj;
    }

    private static SimplePackElement CreateTemporaryAbilityGranter(string path, string ability, bool singleUse, string name)
    {
        var sprite = WeSpriteUtils.Load(path);
        sprite.texture.filterMode = FilterMode.Point;
        
        var granterObj = new GameObject(ability + " Granter");
        
        var renderer = granterObj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        var granter = granterObj.AddComponent<TemporaryAbilityGranter>();
        granter.abilityType = ability;
        granter.singleUse = singleUse;

        var collider = granterObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        
        granterObj.SetActive(false);
        Object.DontDestroyOnLoad(granterObj);

        granterObj.transform.localScale *= 7;

        return new SimplePackElement(granterObj, name, "Custom");
    }

    private static void InitializeAbilityGranterHooks()
    {
        On.HeroController.CanDash += (orig, self) => TemporaryAbilities.Contains("Dash") || orig(self);
        On.HeroController.HeroDash += (orig, self) =>
        {
            orig(self);
            TemporaryAbilities.Remove("Dash");
        };
        On.HeroController.CanDoubleJump += (orig, self) => TemporaryAbilities.Contains("Wings") || orig(self);
        On.HeroController.DoDoubleJump += (orig, self) =>
        {
            orig(self);
            TemporaryAbilities.Remove("Wings");
        };
    }
}