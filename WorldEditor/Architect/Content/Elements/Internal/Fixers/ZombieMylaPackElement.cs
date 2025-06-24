using System.Collections.Generic;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class ZombieMylaPackElement : InternalPackElement
{
    private GameObject _gameObject;

    public ZombieMylaPackElement() : base("Husk Myla", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_45", "Zombie Myla"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_45"]["Zombie Myla"];

        _gameObject.RemoveComponent<DeactivateIfPlayerdataFalse>();
    }
}