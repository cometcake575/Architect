using System.Collections.Generic;
using Architect.Attributes;
using Architect.Content.Groups;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class HornetElement : InternalPackElement
{
    private GameObject _gameObject;

    public HornetElement(int weight) : base("Hornet NPC", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.RepeatNpcs);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
        FlipHorizontal();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Abyss_06_Core", "Hornet Abyss NPC"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Abyss_06_Core"]["Hornet Abyss NPC"];
        foreach (var fsm in _gameObject.GetComponents<PlayMakerFSM>())
            if (fsm.FsmName == "FSM")
                fsm.enabled = false;

        _gameObject.AddComponent<HornetEditor>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.LocateMyFSM("Conversation Control").InsertCustomAction("End",
            makerFsm => { EventManager.BroadcastEvent(makerFsm.gameObject, "OnConvoEnd"); }, 0);
    }

    private class HornetEditor : NpcEditor
    {
        public override string SetFirstConvo()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            var id = "Custom Hornet First " + gameObject.name;
            fsm.GetAction<CallMethodProper>("Greet", 3).parameters[0].stringValue = id;
            return id;
        }

        public override string SetRepeatConvo()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            var id = "Custom Hornet Repeat " + gameObject.name;
            fsm.GetAction<CallMethodProper>("Repeat", 1).parameters[0].stringValue = id;

            return id;
        }
    }
}

internal class QuirrelElement : InternalPackElement
{
    private GameObject _gameObject;

    public QuirrelElement(int weight) : base("Quirrel", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.Npcs);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
        FlipHorizontal();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Room_temple", "Quirrel"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Room_temple"]["Quirrel"];
        foreach (var fsm in _gameObject.GetComponents<PlayMakerFSM>())
            if (fsm.FsmName == "FSM")
                fsm.enabled = false;
        _gameObject.AddComponent<QuirrelEditor>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.LocateMyFSM("Conversation Control").InsertCustomAction("End",
            makerFsm => { EventManager.BroadcastEvent(makerFsm.gameObject, "OnConvoEnd"); }, 0);
    }

    private class QuirrelEditor : NpcEditor
    {
        public override string SetFirstConvo()
        {
            var id = name + " Quirrel Convo";

            var fsm = gameObject.LocateMyFSM("Conversation Control");
            fsm.InsertCustomAction("Convo Choice", makerFsm => makerFsm.SendEvent("TEMPLE"), 0);
            fsm.InsertCustomAction("Egg Temple", makerFsm => makerFsm.SendEvent("EGG 1"), 0);
            fsm.GetAction<CallMethodProper>("Egg Temple 1", 4).parameters[0].stringValue = id;
            fsm.DisableAction("Egg Temple 1", 0);

            return id;
        }
    }
}

internal class ElderbugElement : InternalPackElement
{
    private GameObject _gameObject;

    public ElderbugElement(int weight) : base("Elderbug", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.Npcs);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
        FlipHorizontal();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Town", "_NPCs/Elderbug"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Town"]["_NPCs/Elderbug"];
        _gameObject.AddComponent<ElderbugEditor>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Conversation Control");
        fsm.DisableAction("Grimm?", 0);
        fsm.InsertCustomAction("End", makerFsm => { EventManager.BroadcastEvent(makerFsm.gameObject, "OnConvoEnd"); },
            0);
    }

    private class ElderbugEditor : NpcEditor
    {
        public override string SetFirstConvo()
        {
            var id = name + " Elderbug Convo";

            var fsm = gameObject.LocateMyFSM("Conversation Control");
            fsm.InsertCustomAction("Convo Choice", makerFsm => makerFsm.SendEvent("KINGS PASS"), 0);
            fsm.GetAction<CallMethodProper>("Kings Pass", 2).parameters[0].stringValue = id;
            fsm.DisableAction("Kings Pass", 0);

            var act = fsm.GetAction<CheckTargetDirection>("Check Direction 2", 0);
            if (gameObject.transform.localScale.x < 0)
                (act.leftEvent, act.rightEvent) = (act.rightEvent, act.leftEvent);

            return id;
        }
    }
}

internal class ZoteElement : InternalPackElement
{
    private GameObject _gameObject;

    public ZoteElement(int weight) : base("Zote NPC", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.Npcs);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
        FlipHorizontal();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Town", "_NPCs/Zote Town"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Town"]["_NPCs/Zote Town"];
        _gameObject.AddComponent<ZoteEditor>();
        for (var _ = 0; _ < 3; _++) _gameObject.RemoveComponent<DeactivateIfPlayerdataTrue>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Conversation Control");
        fsm.InsertCustomAction("End", makerFsm => { EventManager.BroadcastEvent(makerFsm.gameObject, "OnConvoEnd"); },
            0);
        fsm.DisableAction("Check Active", 1);
        fsm.DisableAction("Check Active", 2);
    }

    private class ZoteEditor : NpcEditor
    {
        public override string SetFirstConvo()
        {
            var id = name + " Zote Convo";

            var fsm = gameObject.LocateMyFSM("Conversation Control");
            fsm.InsertCustomAction("Convo Choice", makerFsm => makerFsm.SendEvent("1"), 0);
            fsm.GetAction<CallMethodProper>("1", 1).parameters[0].stringValue = id;
            fsm.DisableAction("1", 2);

            var act = fsm.GetAction<CheckTargetDirection>("Check Direction 2", 0);
            if (gameObject.transform.localScale.x < 0)
                (act.leftEvent, act.rightEvent) = (act.rightEvent, act.leftEvent);

            return id;
        }
    }
}

internal class GodseekerElement : InternalPackElement
{
    private GameObject _gameObject;

    public GodseekerElement(int weight) : base("Godseeker", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.RepeatNpcs);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
        FlipHorizontal();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("GG_Engine", "Godseeker EngineRoom NPC"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["GG_Engine"]["Godseeker EngineRoom NPC"];
        _gameObject.AddComponent<GodseekerEditor>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.LocateMyFSM("Conversation Control").InsertCustomAction("End",
            makerFsm => { EventManager.BroadcastEvent(makerFsm.gameObject, "OnConvoEnd"); }, 0);
    }

    private class GodseekerEditor : NpcEditor
    {
        public override string SetFirstConvo()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            var id = "Custom Godseeker First " + gameObject.name;

            fsm.AddCustomAction("Init", makerFsm =>
            {
                makerFsm.FsmVariables.FindFsmBool("Door Completed").Value = false;
                makerFsm.FsmVariables.FindFsmString("First Cell").Value = id;
            });
            return id;
        }

        public override string SetRepeatConvo()
        {
            var fsm = gameObject.LocateMyFSM("Conversation Control");
            var id = "Custom Godseeker Repeat " + gameObject.name;

            fsm.AddCustomAction("Init", makerFsm =>
            {
                makerFsm.FsmVariables.FindFsmBool("Door Completed").Value = false;
                makerFsm.FsmVariables.FindFsmString("Repeat Cell").Value = id;
            });
            return id;
        }
    }
}

internal class MidwifeElement : InternalPackElement
{
    private GameObject _gameObject;

    public MidwifeElement(int weight) : base("Midwife", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Midwife);
        WithBroadcasterGroup(BroadcasterGroup.Npcs);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Deepnest_41", "Happy Spider NPC"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Deepnest_41"]["Happy Spider NPC"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var go = gameObject.transform.GetChild(1).gameObject;
        go.LocateMyFSM("Conversation Control").InsertCustomAction("End",
            makerFsm => { EventManager.BroadcastEvent(gameObject, "OnConvoEnd"); }, 0);

        if (flipped == Mathf.Approximately(rotation, 180)) return;
        go.LocateMyFSM("npc_control").FsmVariables
            .FindFsmFloat("Move To Offset")
            .Value = -3;
    }
}

public abstract class NpcEditor : MonoBehaviour
{
    public abstract string SetFirstConvo();

    public virtual string SetRepeatConvo()
    {
        return null;
    }
}