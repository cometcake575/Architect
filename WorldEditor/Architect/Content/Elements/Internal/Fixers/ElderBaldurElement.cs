using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ElderBaldurElement : InternalPackElement
{
    private GameObject _gameObject;

    public ElderBaldurElement() : base("Elder Baldur", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        FlipHorizontal();
    }

    public override bool DisableScaleParent()
    {
        return true;
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_ShamanTemple", "Battle Scene/Blocker"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_ShamanTemple"]["Battle Scene/Blocker"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        gameObject.transform.SetScaleX(-_gameObject.transform.GetScaleX());
        if (flipped) return;
        var fsm = gameObject.LocateMyFSM("Blocker Control");
        fsm.FsmVariables.GetFsmBool("Facing Right").Value = false;
    }
}