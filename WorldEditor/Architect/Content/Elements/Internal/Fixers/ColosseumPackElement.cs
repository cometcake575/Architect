using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ColosseumPackElement : InternalPackElement
{
    private GameObject _gameObject;
    private readonly string _scene;
    private readonly string _path;

    public ColosseumPackElement(string scene, string path, string name, string category, int weight = 0) : base(name, category, weight)
    {
        _scene = scene;
        _path = path;
        WithConfigGroup(ConfigGroup.Enemies);
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add((_scene, _path));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        var cage = preloads[_scene][_path];

        var fsm = cage.LocateMyFSM("Spawn");
        
        var corpse = fsm.FsmVariables.FindFsmGameObject("Corpse to Instantiate");
        if (corpse != null)
        {
            _gameObject = corpse.Value;
            return;
        }
        
        _gameObject = fsm.FsmVariables.FindFsmGameObject("Enemy Type").Value;
    }
}