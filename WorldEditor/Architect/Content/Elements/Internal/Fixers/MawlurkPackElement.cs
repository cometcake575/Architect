using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class MawlurkPackElement : InternalPackElement
{
    private GameObject _normal;
    private GameObject _inverted;

    public MawlurkPackElement() : base("Mawlurk", "Enemies")
    {
        WithRotationGroup(RotationGroup.Vertical);
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override bool OverrideRotation()
    {
        return true;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return rotation == 180 ? _inverted : _normal;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Abyss_20", "Mawlek Turret"));
        preloadInfo.Add(("Abyss_20", "Mawlek Turret Ceiling"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _normal = preloads["Abyss_20"]["Mawlek Turret"];
        _inverted = preloads["Abyss_20"]["Mawlek Turret Ceiling"];
    }
}