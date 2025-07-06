using System.Collections.Generic;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Content.Groups;
using HutongGames.PlayMaker;
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
        {
            if (fsm.FsmName == "FSM") fsm.enabled = false;
        }

        _gameObject.AddComponent<HornetEditor>();
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

internal class GodseekerElement : InternalPackElement
{
    private GameObject _gameObject;

    public GodseekerElement(int weight) : base("Godseeker", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.RepeatNpcs);
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

public abstract class NpcEditor : MonoBehaviour
{
    public abstract string SetFirstConvo();
    
    public abstract string SetRepeatConvo();
}