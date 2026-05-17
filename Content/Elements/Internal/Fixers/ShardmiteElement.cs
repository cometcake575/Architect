using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class ShardmiteElement : InternalPackElement
{
    private GameObject _gameObject;

    public ShardmiteElement() : base("Shardmite", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithRotationGroup(RotationGroup.Four);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Mines_20", "Mines Crawler"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Mines_20"]["Mines Crawler"];
        _gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public override bool OverrideRotation()
    {
        return true;
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        base.PostSpawn(gameObject, flipped, rotation, scale);
        if (Mathf.Approximately(rotation, 180) || Mathf.Approximately(rotation, 270))
        {
            gameObject.transform.localScale = -gameObject.transform.localScale;
            rotation -= 180;
        }

        var rot = gameObject.transform.rotation.eulerAngles;
        rot.z = rotation;
        gameObject.transform.rotation = Quaternion.Euler(rot);
    }
}