using System.Collections.Generic;
using Architect.Content.Groups;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class FallingCrystalsElement : InternalPackElement
{
    private GameObject _gameObject;
    private Sprite _sprite;

    public FallingCrystalsElement() : base("Falling Crystals", "Decorations")
    {
        WithConfigGroup(ConfigGroup.FallingCrystals);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Mines_31", "Pt Crystal Dropping (13)"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Mines_31"]["Pt Crystal Dropping (13)"];
        _sprite = ResourceUtils.Load("falling_crystal");
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var ps = gameObject.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startSizeMultiplier *= scale;
        main.startRotationMultiplier = rotation;

        if (flipped)
        {
            var vol = ps.velocityOverLifetime;
            vol.xMultiplier = -vol.xMultiplier;

            var volX = vol.x;
            volX.constantMin = -volX.constantMin;
            volX.constantMax = -volX.constantMax;
            volX.constant = -volX.constant;
            vol.x = volX;
        }
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }
}

internal class DamagingCrystals : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if (other.layer == 9) HeroController.instance.TakeDamage(null, 0, 1, 1);
    }
}
