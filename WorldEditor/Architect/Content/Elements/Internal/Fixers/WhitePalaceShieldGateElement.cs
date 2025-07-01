using System.Collections.Generic;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class WhitePalaceShieldGateElement : InternalPackElement
{
    private GameObject _gameObject;

    public WhitePalaceShieldGateElement(int weight) : base("White Palace Shield Gate", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithReceiverGroup(ReceiverGroup.BattleGate);
        WithConfigGroup(ConfigGroup.BattleGate);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("White_Palace_20", "Battle Scene/Gate"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["White_Palace_20"]["Battle Scene/Gate"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var collider = gameObject.GetComponent<BoxCollider2D>();
        collider.offset = new Vector2(0.25f, 0.2165f);
        collider.size = new Vector2(0.25f, 3.5f);

        var fsm = gameObject.LocateMyFSM("FSM");
        fsm.AddCustomAction("Downed", makerFsm => makerFsm.GetComponent<BoxCollider2D>().enabled = false);
        fsm.AddCustomAction("Upped", makerFsm => makerFsm.GetComponent<BoxCollider2D>().enabled = true);
    }
}