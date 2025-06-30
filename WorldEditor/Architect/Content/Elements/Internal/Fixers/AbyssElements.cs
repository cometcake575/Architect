using System.Collections.Generic;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ShadeSiblingElement : InternalPackElement
{
    private GameObject _gameObject;

    public ShadeSiblingElement() : base("Shade Sibling", "Enemies")
    {
        WithConfigGroup(ConfigGroup.VisibleAbyss);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Abyss_06_Core", "Shade Sibling Spawner"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Abyss_06_Core"]["Shade Sibling Spawner"].GetComponent<PersonalObjectPool>().startupPool[0].prefab;

        _gameObject.AddComponent<ShadeSiblingVhEffects>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        var pos = gameObject.transform.position;
        pos.y -= 6.5f;
        gameObject.transform.position = pos;
    }

    internal class ShadeSiblingVhEffects : VhEffects
    {
        public override void ForceDisable()
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.DisableAction("Friendly?", 2);
            fsm.DisableAction("Friendly?", 3);
            fsm.DisableAction("Friendly?", 4);
            fsm.DisableAction("Friendly?", 5);
            fsm.DisableAction("Friendly?", 6);
            fsm.DisableAction("Friendly?", 7);
        }
    }
}

internal class VoidTendrilsElement : InternalPackElement
{
    private GameObject _gameObject;

    public VoidTendrilsElement() : base("Void Tendrils", "Hazards")
    {
        WithRotationGroup(RotationGroup.Eight);
        WithConfigGroup(ConfigGroup.Abyss);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Abyss_16", "Abyss Tendrils"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Abyss_16"]["Abyss Tendrils"];
        _gameObject.AddComponent<VoidTendrilsVhEffects>();
    }

    internal class VoidTendrilsVhEffects : VhEffects
    {
        public override void ForceDisable()
        { 
            gameObject.LocateMyFSM("Black Charm").enabled = false;
        }
    }
}

internal abstract class VhEffects : MonoBehaviour
{
    public abstract void ForceDisable();
}