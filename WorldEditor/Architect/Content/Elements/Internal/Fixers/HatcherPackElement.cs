using System.Collections.Generic;
using Architect.Content.Groups;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class HatcherPackElement : GInternalPackElement
{
    private GameObject _cageObject;
    
    private readonly string _scene;
    private readonly string _cagePath;
    private readonly string _fsmName;

    public HatcherPackElement(string scene, string path, string cagePath, string name, string fsmName) : base(scene, path, name, "Enemies", 0)
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        
        _scene = scene;
        _cagePath = cagePath;
        
        _fsmName = fsmName;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        base.AddPreloads(preloadInfo);
        preloadInfo.Add((_scene, _cagePath));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        Initialize();
        _cageObject = preloads[_scene][_cagePath];
    }
    
    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        var cage = Object.Instantiate(_cageObject);
        cage.SetActive(true);
        cage.transform.localScale *= scale;
        cage.name = gameObject.name + " Cage";

        var fsm = gameObject.LocateMyFSM(_fsmName);
        if (!fsm.TryGetState("Init", out var state))
        {
            state = fsm.GetValidState("Initiate");
        }

        var fgo = state.GetAction<FindGameObject>(1) ?? state.GetAction<FindGameObject>(2);
        fgo.objectName = cage.name;
        
        gameObject.AddComponent<PlacedHatcher>().cage = cage;
    }

    private static bool _init;
    
    // Modifies the death effects so that the correct enemy spawns upon killing the hatcher
    private static void Initialize()
    {
        if (_init) return;
        _init = true;

        On.EnemyDeathEffects.PreInstantiate += (orig, self) =>
        {
            orig(self);
            
            // Copies placed hatcher info to the corpse
            self.GetComponentInChildren<Corpse>(true)
                .gameObject.AddComponent<PlacedHatcher>().cage = self.GetComponent<PlacedHatcher>().cage;
        };
        
        On.CorpseHatcher.Smash += (orig, self) =>
        {
            var hatcher = self.GetComponent<PlacedHatcher>();
            if (hatcher)
            {
                var toEnable = DisableOthers(hatcher);
                orig(self);
                EnableOthers(toEnable);
            }
            else orig(self);
        };
        
        On.CorpseZomHive.LandEffects += (orig, self) =>
        {
            var hatcher = self.GetComponent<PlacedHatcher>();
            if (hatcher)
            {
                var toEnable = DisableOthers(hatcher);
                orig(self);
                EnableOthers(toEnable);
            }
            else orig(self);
        };
    }

    private static void EnableOthers(List<GameObject> toEnable)
    {
        foreach (var obj in toEnable) obj.tag = "Extra Tag";
    }

    private static List<GameObject> DisableOthers(PlacedHatcher hatcher)
    {
        List<GameObject> toEnable = new();
        if (!hatcher) return toEnable;
        foreach (var obj in GameObject.FindGameObjectsWithTag("Extra Tag"))
        {
            if (obj == hatcher.cage || !obj.activeInHierarchy) continue;
            obj.tag = "Untagged";
            toEnable.Add(obj);
        }
        return toEnable;
    }

    private class PlacedHatcher : MonoBehaviour
    {
        public GameObject cage;
    }
}