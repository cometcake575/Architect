using System.Collections.Generic;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class MenderbugElement : InternalPackElement
{
    private GameObject _gameObject;

    public MenderbugElement() : base("Menderbug", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_01", "_Scenery/Mender Bug"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_01"]["_Scenery/Mender Bug"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Mender Bug Ctrl");
        fsm.SetState("Idle");
        fsm.DisableAction("Killed", 0);
    }
}