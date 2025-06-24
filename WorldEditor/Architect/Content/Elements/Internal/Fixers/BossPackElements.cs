using System.Collections.Generic;
using Architect.Content.Groups;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class VengeflyKingElement : InternalPackElement
{
    private GameObject _gameObject;

    public VengeflyKingElement() : base("Vengefly King", "Bosses")
    {
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Vengefly", "Giant Buzzer Col"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Vengefly"]["Giant Buzzer Col"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Big Buzzer");
        
        fsm.DisableAction("Init", 3);
        
        var pos = gameObject.transform.position;
        Architect.Instance.Log(pos);
        
        fsm.AddAction("Title?", new SetFloatValue
        {
            floatVariable = new FsmFloat("Swoop Height"),
            floatValue = pos.y - 3.28f
        });
        
        fsm.InsertCustomAction("Swoop Direction", makerFsm =>
        {
            Architect.Instance.Log(makerFsm.FsmVariables.GetFsmFloat("Swoop Height"));
            Architect.Instance.Log(makerFsm.FsmVariables.GetFsmFloat("Swoop Target"));
        }, 6);
        
        fsm.GetAction<CreateObject>("Summon", 1).position = new Vector3(pos.x - 17.48f, pos.y + 7.72f, pos.z + 16.99f);
        fsm.GetAction<CreateObject>("Summon", 3).position = new Vector3(pos.x + 1.52f, pos.y + 7.72f, pos.z + 16.99f);
    }
}
