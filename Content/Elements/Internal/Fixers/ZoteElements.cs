using System.Collections.Generic;
using Architect.Attributes;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ZotelingElement : InternalPackElement
{
    private readonly string _path;
    protected GameObject GameObject;

    public ZotelingElement(string path, string name) : base(name, "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
        WithReceiverGroup(ReceiverGroup.Enemies);

        _path = path;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return GameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mighty_Zote", "Battle Control/" + _path));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        GameObject = preloads["GG_Mighty_Zote"]["Battle Control/" + _path];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var control = gameObject.LocateMyFSM("Control");
        for (var i = 1; i <= 5; i++) control.DisableAction("Spawn Antic", i);
        control.AddCustomAction("Dormant", fsm => fsm.SendEvent("SPAWN"));
        gameObject.GetComponent<HealthManager>().hasSpecialDeath = false;
    }
}

internal class BallZotelingElement : InternalPackElement
{
    private readonly string _type;
    private GameObject _gameObject;

    public BallZotelingElement(string name, string type) : base(name, "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
        _type = type;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mighty_Zote", "Battle Control/Zotelings/Ordeal Zoteling"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mighty_Zote"]["Battle Control/Zotelings/Ordeal Zoteling"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var control = gameObject.LocateMyFSM("Control");
        control.DisableAction("Ball", 2);
        var random = control.GetAction<WaitRandom>("Ball", 6);
        random.timeMin = 0.0001f;
        random.timeMax = 0.001f;

        control.AddCustomAction("Dormant", fsm => fsm.SendEvent("SPAWN"));
        gameObject.GetComponent<HealthManager>().hasSpecialDeath = false;

        control.DisableAction("Choice", 3);
        control.AddCustomAction("Choice", fsm => fsm.SendEvent(_type));
    }
}

internal sealed class TallZotelingElement : ZotelingElement
{
    public TallZotelingElement() : base("Tall Zotes/Zote Crew Tall (1)", "Lanky Zoteling")
    {
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);

        var constrain = GameObject.GetComponent<ConstrainPosition>();
        if (constrain) constrain.enabled = false;
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        base.PostSpawn(gameObject, flipped, rotation, scale);

        var control = gameObject.LocateMyFSM("Control");
        var posSet = control.GetAction<SetPosition>("Grav", 1);
        posSet.Enabled = false;
    }
}

internal sealed class FatZotelingElement : ZotelingElement
{
    public FatZotelingElement() : base("Fat Zotes/Zote Crew Fat (1)", "Heavy Zoteling")
    {
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        base.PostSpawn(gameObject, flipped, rotation, scale);

        var control = gameObject.LocateMyFSM("Control");
        control.InsertCustomAction("Land Waves",
            fsm => { fsm.FsmVariables.FindFsmFloat("Shockwave Y").Value = fsm.transform.position.y - 2.3516f; }, 2);
    }
}

internal sealed class VolatileZotelingElement : InternalPackElement
{
    private GameObject _gameObject;

    public VolatileZotelingElement() : base("Volatile Zoteling", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.VolatileZoteling);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var control = gameObject.LocateMyFSM("Control");
        control.AddCustomAction("Dormant", fsm => fsm.SendEvent("BALLOON SPAWN"));
        gameObject.GetComponent<HealthManager>().hasSpecialDeath = false;

        control.DisableAction("Set Pos", 6);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mighty_Zote", "Battle Control/Zote Balloon Ordeal"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mighty_Zote"]["Battle Control/Zote Balloon Ordeal"];
    }
}

internal sealed class FlukeZotelingElement : InternalPackElement
{
    private GameObject _gameObject;

    public FlukeZotelingElement() : base("Fluke Zoteling", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.FlukeZoteling);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var control = gameObject.LocateMyFSM("Control");
        control.AddCustomAction("Dormant", fsm => fsm.SendEvent("GO"));
        gameObject.GetComponent<HealthManager>().hasSpecialDeath = false;

        control.DisableAction("Pos", 3);

        control.GetAction<FloatCompare>("Climb", 3).float2 = gameObject.transform.position.y + 18.88f;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mighty_Zote", "Battle Control/Zote Fluke"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mighty_Zote"]["Battle Control/Zote Fluke"];
    }
}

internal sealed class ZoteCurseElement : InternalPackElement
{
    private GameObject _gameObject;

    public ZoteCurseElement() : base("Zote's Curse", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.KillableEnemies);
        WithReceiverGroup(ReceiverGroup.Enemies);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var control = gameObject.LocateMyFSM("Control");
        control.AddCustomAction("Dormant", fsm => fsm.SendEvent("START"));
        gameObject.GetComponent<HealthManager>().hasSpecialDeath = false;

        control.DisableAction("Appear", 3);
        control.DisableAction("Appear", 6);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Mighty_Zote", "Battle Control/Zote Salubra"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Mighty_Zote"]["Battle Control/Zote Salubra"];
    }
}

internal class ZoteHeadElement : InternalPackElement
{
    private GameObject _gameObject;

    public ZoteHeadElement(int weight) : base("Zote Head", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.All);
        WithConfigGroup(ConfigGroup.ZoteHead);
        WithBroadcasterGroup(BroadcasterGroup.ZoteHead);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus1_20_v02", "Zote Death/Head"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Fungus1_20_v02"]["Zote Death/Head"];
        _gameObject.AddComponent<ZoteHead>();
        _gameObject.AddComponent<PngObject>();
        _gameObject.AddComponent<ConveyorMovement>();
        _gameObject.transform.SetRotation2D(0);
    }
}

internal class ZoteHead : MonoBehaviour
{
    private Collider2D _col2d;
    private bool _ground;

    private void Start()
    {
        _col2d = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D _)
    {
        if (CheckTouchingGround())
        {
            _ground = true;
            EventManager.BroadcastEvent(gameObject, "Land");
        }
    }

    private void OnCollisionExit2D(Collision2D _)
    {
        if (_ground && !CheckTouchingGround())
        {
            _ground = false;
            EventManager.BroadcastEvent(gameObject, "InAir");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponentInChildren<NailSlash>()) EventManager.BroadcastEvent(gameObject, "OnHit");
    }

    public bool CheckTouchingGround()
    {
        var bounds1 = _col2d.bounds;
        double x1 = bounds1.min.x;
        bounds1 = _col2d.bounds;
        double y1 = bounds1.center.y;
        var vector21 = new Vector2((float)x1, (float)y1);
        Vector2 center = _col2d.bounds.center;
        var bounds2 = _col2d.bounds;
        double x2 = bounds2.max.x;
        bounds2 = _col2d.bounds;
        double y2 = bounds2.center.y;
        var vector22 = new Vector2((float)x2, (float)y2);
        bounds2 = _col2d.bounds;
        var distance = bounds2.extents.y + 0.16f;
        Debug.DrawRay(vector21, Vector2.down, Color.yellow);
        Debug.DrawRay(center, Vector2.down, Color.yellow);
        Debug.DrawRay(vector22, Vector2.down, Color.yellow);
        var raycastHit2D1 = Physics2D.Raycast(vector21, Vector2.down, distance, 256);
        var raycastHit2D2 = Physics2D.Raycast(center, Vector2.down, distance, 256);
        var raycastHit2D3 = Physics2D.Raycast(vector22, Vector2.down, distance, 256);
        return raycastHit2D1.collider || raycastHit2D2.collider || raycastHit2D3.collider;
    }
}