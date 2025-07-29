using System.Collections.Generic;
using Architect.Content.Groups;
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
        WithConfigGroup(ConfigGroup.VengeflyKing);
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
        _gameObject.AddComponent<VkConfig>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var config = gameObject.GetComponent<VkConfig>();
        var targetPlayer = config.targetPlayer;
        var vengeflyRule = config.vengeflyRule;

        if (vengeflyRule == -1) vengeflyRule = targetPlayer ? 2 : 1;
        
        var fsm = gameObject.LocateMyFSM("Big Buzzer");
        
        fsm.DisableAction("Init", 3);
        
        var pos = gameObject.transform.position;
        
        fsm.AddCustomAction("Swoop Antic", makerFsm =>
        {
            makerFsm.FsmVariables.FindFsmFloat("Swoop Height").Value = 
                targetPlayer ? HeroController.instance.transform.position.y + 1
                : pos.y - 3.28f;
        });
        
        var s1 = fsm.GetAction<CreateObject>("Summon", 1);
        var s2 = fsm.GetAction<CreateObject>("Summon", 3);
        switch (vengeflyRule)
        {
            // Disabled
            case 0:
                s1.Enabled = false;
                s2.Enabled = false;
                return;
            // Locked offset
            case 1:
                s1.position = new Vector3(pos.x - 17.48f, pos.y + 7.72f, pos.z + 16.99f);
                s2.position = new Vector3(pos.x + 1.52f, pos.y + 7.72f, pos.z + 16.99f);
                break;
            // Offset to current VK position
            default:
                s2.storeObject = s1.storeObject;

                fsm.InsertCustomAction("Summon", makerFsm =>
                {
                    var obj = makerFsm.FsmVariables.FindFsmGameObject("Buzzer Instance").Value;
                    var mpos = makerFsm.transform.position;
                    if (obj) obj.transform.position = new Vector3(mpos.x + 20.3926f *
                        (makerFsm.transform.GetScaleX() > 0
                            ? -1
                            : 1), mpos.y + 6.9023f, 17);
                }, 4);
                
                fsm.InsertCustomAction("Summon", makerFsm =>
                {
                    var obj = makerFsm.FsmVariables.FindFsmGameObject("Buzzer Instance").Value;
                    var mpos = makerFsm.transform.position;
                    if (obj) obj.transform.position = new Vector3(mpos.x + 1.3926f *
                                                                            (makerFsm.transform.GetScaleX() > 0
                                                                                ? -1
                                                                                : 1), mpos.y + 6.9023f, 17);
                }, 2);
                
                break;
        }

    }

    internal class VkConfig : MonoBehaviour
    {
        public bool targetPlayer = true;
        
        public int vengeflyRule = -1;
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

        _gameObject.AddComponent<GmConfig>();
    }

    internal class GmConfig : MonoBehaviour
    {
        public bool spawnGruzzers = true;
        private bool _madeChanges;

        private void Update()
        {
            if (_madeChanges) return;
            var child = gameObject.transform.GetChild(3);
            if (child)
            {
                var corpse = child.gameObject.LocateMyFSM("corpse");
                if (spawnGruzzers) corpse.AddCustomAction("Blow", fsm =>
                {
                    _madeChanges = true;
                    var flySpawn = Instantiate(_child);
                    flySpawn.name = gameObject.name + " Fly Spawn";
                    flySpawn.SetActive(true);

                    var makerFsm = fsm.FsmVariables.FindFsmGameObject("Burster").Value.LocateMyFSM("burster");
                    makerFsm
                        .GetAction<FindGameObject>("Initiate", 4)
                        .objectName = flySpawn.name;
                    makerFsm.RemoveAction("Geo", 0);
                });
                else corpse.AddCustomAction("Blow", fsm =>
                {
                    var makerFsm = fsm.FsmVariables.FindFsmGameObject("Burster").Value.LocateMyFSM("burster");
                    var check = makerFsm
                        .GetAction<GGCheckIfBossScene>("Stop", 1);
                    check.regularSceneEvent = check.bossSceneEvent;
                    makerFsm.RemoveAction("Geo", 0);
                });
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
        WithConfigGroup(ConfigGroup.KillableEnemies);
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
        WithConfigGroup(ConfigGroup.KillableEnemies);
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

internal class BroodingMawlekElement : InternalPackElement
{
    private GameObject _gameObject;

    public BroodingMawlekElement() : base("Brooding Mawlek", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Brooding_Mawlek", "Battle Scene/Mawlek Body"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Brooding_Mawlek"]["Battle Scene/Mawlek Body"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.LocateMyFSM("Mawlek Control").SendEvent("GG BOSS");
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
        WithConfigGroup(ConfigGroup.KillableEnemies);
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
        
        fsm.AddCustomAction("Sleep", makerFsm =>
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

internal class BrokenVesselElement : GInternalPackElement
{
    public BrokenVesselElement(string scene, string path, string name) : base(scene, path, name, "Enemies", 0)
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.BrokenVessel);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return GameObject;
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        GameObject.AddComponent<BrokenVesselConfig>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var config = gameObject.GetComponent<BrokenVesselConfig>();
        
        var fsm = gameObject.LocateMyFSM("IK Control");
        
        if (fsm.TryGetState("Set Pos", out var state)) state.DisableAction(1);
        else
        {
            fsm.DisableAction("Set X", 0);
            fsm.DisableAction("Set X", 2);
            fsm.DisableAction("Intro Fall", 2);
        }

        foreach (var act in fsm.GetActions<PlayerDataBoolTest>("Init")) act.isFalse = act.isTrue;
        
        var waiting = fsm.GetAction<BoolTest>("Waiting", 3);
        waiting.isFalse = waiting.isTrue;
        
        fsm.DisableAction("Close Gates", 0);
        
        if (config.disableRoar) fsm.DisableAction("Roar", 5);

        var pos = fsm.gameObject.transform.position;

        fsm.FsmVariables.FindFsmFloat("Min Dstab Height").Value = -100;

        var aimJump2 = fsm.GetAction<RandomFloat>("Aim Jump 2", 0);
        
        if (config.localJump)
        {
            fsm.InsertCustomAction("Aim Jump", makerFsm =>
            {
                var newPos = makerFsm.gameObject.transform.position;
                makerFsm.FsmVariables.FindFsmFloat("Left X").Value = newPos.x - 10.235f;
                makerFsm.FsmVariables.FindFsmFloat("Right X").Value = newPos.x + 10.235f;
            }, 0);

            aimJump2.min = fsm.FsmVariables.FindFsmFloat("Left X");
            aimJump2.max = fsm.FsmVariables.FindFsmFloat("Right X");
        }
        else
        {
            fsm.FsmVariables.FindFsmFloat("Left X").Value = pos.x - 10.235f;
            fsm.FsmVariables.FindFsmFloat("Right X").Value = pos.x + 10.235f;
            
            aimJump2.min = pos.x - 1;
            aimJump2.max = pos.x + 1;
        }
        
        fsm.DisableAction("Set Height", 0);

        var balloonFsm = gameObject.LocateMyFSM("Spawn Balloon");
        switch (config.balloons)
        {
            case 0:
                balloonFsm.InsertCustomAction("Spawn", makerFsm =>
                {
                    var newPos = makerFsm.gameObject.transform.position;
                    makerFsm.FsmVariables.FindFsmFloat("X Min").Value = newPos.x - 9.55f;
                    makerFsm.FsmVariables.FindFsmFloat("X Max").Value = newPos.x + 9.55f;
                
                    makerFsm.FsmVariables.FindFsmFloat("Y Min").Value = newPos.y;
                    makerFsm.FsmVariables.FindFsmFloat("Y Max").Value = newPos.y + 5.26f;
                }, 0);
                break;
            case 1:
                fsm.FsmVariables.FindFsmFloat("X Min").Value = pos.x - 9.55f;
                fsm.FsmVariables.FindFsmFloat("X Max").Value = pos.x + 9.55f;
                
                fsm.FsmVariables.FindFsmFloat("Y Min").Value = pos.y;
                fsm.FsmVariables.FindFsmFloat("Y Max").Value = pos.y + 5.26f;
                break;
            default:
                balloonFsm.RemoveState("Spawn");
                break;
        }
    }

    public class BrokenVesselConfig : MonoBehaviour
    {
        public bool localJump = true;
        
        public int balloons;
        
        public bool disableRoar = true;
    }
}


internal class PureVesselElement : InternalPackElement
{
    private GameObject _gameObject;

    public PureVesselElement() : base("Pure Vessel", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
        FlipVertical();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Hollow_Knight", "Battle Scene/HK Prime"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Hollow_Knight"]["Battle Scene/HK Prime"];
        _gameObject.RemoveComponent<ConstrainPosition>();
    }
}

internal class MassiveMossChargerElement : InternalPackElement
{
    private GameObject _gameObject;

    public MassiveMossChargerElement() : base("Massive Moss Charger", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Awakable);
        WithConfigGroup(ConfigGroup.MassiveMc);
        FlipVertical();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mega_Moss_Charger", "Mega Moss Charger"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mega_Moss_Charger"]["Mega Moss Charger"];
        _gameObject.RemoveComponent<BoxCollider2D>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Mossy Control");
        fsm.DisableAction("Init", 28);

        fsm.GetAction<SetScale>("Emerge Left", 6).x = scale;
        fsm.GetAction<SetScale>("Emerge Right", 6).x = -scale;
    }

    public override bool DisableScaleParent()
    {
        return true;
    }
}


internal class HornetProtectorElement : InternalPackElement
{
    private GameObject _gameObject;

    public HornetProtectorElement() : base("Hornet Protector", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithReceiverGroup(ReceiverGroup.Awakable);
        WithConfigGroup(ConfigGroup.MassiveMc);
        FlipVertical();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mega_Moss_Charger", "Mega Moss Charger"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mega_Moss_Charger"]["Mega Moss Charger"];
        _gameObject.RemoveComponent<BoxCollider2D>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Mossy Control");
        fsm.DisableAction("Init", 28);

        fsm.GetAction<SetScale>("Emerge Left", 6).x = scale;
        fsm.GetAction<SetScale>("Emerge Right", 6).x = -scale;
    }

    public override bool DisableScaleParent()
    {
        return true;
    }
}
