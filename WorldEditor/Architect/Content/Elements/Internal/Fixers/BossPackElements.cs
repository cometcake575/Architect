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

    public VengeflyKingElement() : base("Vengefly King", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
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

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
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

internal class GruzMotherElement : InternalPackElement
{
    private GameObject _gameObject;

    public GruzMotherElement() : base("Gruz Mother", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_04", "_Enemies/Giant Fly"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_04"]["_Enemies/Giant Fly"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
    }
}

internal class OblobbleElement : InternalPackElement
{
    private GameObject _gameObject;

    public OblobbleElement() : base("Oblobble", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Oblobbles", "Mega Fat Bee"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Oblobbles"]["Mega Fat Bee"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("fat fly bounce");

        fsm.GetAction<iTweenMoveBy>("Swoop In", 7).vector.Value = new Vector3(0, -15, 0);
    }
}

internal class TamerBeastElement : InternalPackElement
{
    private GameObject _gameObject;

    public TamerBeastElement() : base("God Tamer Beast", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Room_Colosseum_Gold", "Colosseum Manager/Waves/Lobster Lancer/Entry Object/Lobster"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Room_Colosseum_Gold"]["Colosseum Manager/Waves/Lobster Lancer/Entry Object/Lobster"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.LocateMyFSM("Control").SendEvent("WAKE");
    }
}

internal class WatcherKnightElement : InternalPackElement
{
    private GameObject _gameObject;

    public WatcherKnightElement() : base("Watcher Knight", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.WatcherKnights);
        WithConfigGroup(ConfigGroup.WatcherKnights);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Watcher_Knights", "Battle Control/Black Knight 1"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Watcher_Knights"]["Battle Control/Black Knight 1"];
        _gameObject.transform.SetPositionZ(0.1102f);
        for (var i = 0; i < 4; i++) _gameObject.transform.GetChild(i).gameObject.SetActive(false);
    }
}

internal class GrimmElement : InternalPackElement
{
    private GameObject _gameObject;

    public GrimmElement() : base("Troupe Master Grimm", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Grimm", "Grimm Scene"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Grimm"]["Grimm Scene"];
    }
}
