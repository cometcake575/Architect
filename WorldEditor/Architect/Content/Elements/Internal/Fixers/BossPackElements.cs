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
    private static GameObject _child;

    public GruzMotherElement() : base("Gruz Mother", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Awakable);
        WithConfigGroup(ConfigGroup.GruzMother);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_04", "_Enemies/Giant Fly"));
        preloadInfo.Add(("Crossroads_04", "_Enemies/Fly Spawn"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_04"]["_Enemies/Giant Fly"];
        _child = preloads["Crossroads_04"]["_Enemies/Fly Spawn"];
        
        var child = _gameObject.transform.GetChild(2).gameObject;
        child.gameObject.RemoveComponent<PolygonCollider2D>();
        var box = child.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size *= 10;

        _gameObject.AddComponent<GruzMotherConfig>();
    }

    internal class GruzMotherConfig : MonoBehaviour
    {
        public bool spawnGruzzers = true;

        private void Update()
        {
            if (!spawnGruzzers) return;
            var child = gameObject.transform.GetChild(3);
            if (child)
            {
                var flySpawn = Instantiate(_child);
                flySpawn.name = gameObject.name + " Fly Spawn";
                flySpawn.SetActive(true);
                
                child.gameObject.LocateMyFSM("burster").GetAction<FindGameObject>("Initiate", 4)
                    .objectName = flySpawn.name;
                spawnGruzzers = false;
            }
        }
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
        WithReceiverGroup(ReceiverGroup.Awakable);
        WithConfigGroup(ConfigGroup.Awakable);
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

internal class SoulWarriorElement : InternalPackElement
{
    private GameObject _gameObject;

    public SoulWarriorElement() : base("Soul Warrior", "Enemies")
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
        preloadInfo.Add(("GG_Mage_Knight", "Mage Knight"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mage_Knight"]["Mage Knight"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var body = gameObject.GetComponent<Rigidbody2D>();
        var fsm = gameObject.LocateMyFSM("Mage Knight");
        
        fsm.AddCustomAction("Rest", makerFsm =>
        {
            makerFsm.SendEvent("WAKE");
        });
        
        fsm.InsertCustomAction("Side Tele Aim", makerFsm =>
        {
            makerFsm.FsmVariables.FindFsmFloat("Floor Y").Value = makerFsm.gameObject.transform.position.y;
        }, 0);
        fsm.InsertCustomAction("Up Tele Aim", _ =>
        {
            body.gravityScale = 0;
        }, 0);
        fsm.InsertCustomAction("Idle", _ =>
        {
            body.gravityScale = 1;
        }, 0);
    }
}
