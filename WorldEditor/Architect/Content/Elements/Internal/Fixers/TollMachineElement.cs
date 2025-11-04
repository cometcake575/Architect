using System.Collections.Generic;
using Architect.Attributes;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class TollMachineElement : InternalPackElement
{
    private GameObject _gameObject;

    public TollMachineElement(int weight) : base("Toll Machine", "Interactable", weight)
    {
        WithBroadcasterGroup(BroadcasterGroup.Tolls);
        WithConfigGroup(ConfigGroup.Toll);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus1_31", "Toll Gate Machine"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Fungus1_31"]["Toll Gate Machine"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Toll Machine");
        fsm.DisableAction("Open Gates", 1);
        fsm.AddCustomAction("Open Gates", makerFsm => EventManager.BroadcastEvent(makerFsm.gameObject, "OnPay"));
        fsm.AddCustomAction("Open Gates", makerFsm => EventManager.BroadcastEvent(makerFsm.gameObject, "FirstPay"));

        fsm.DisableAction("Activated", 0);
        fsm.AddCustomAction("Open Gates", makerFsm => EventManager.BroadcastEvent(makerFsm.gameObject, "OnPay"));
        fsm.AddCustomAction("Activated", makerFsm => EventManager.BroadcastEvent(makerFsm.gameObject, "LoadedPaid"));
    }
}