using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Darkness : MonoBehaviour
{
    private static readonly List<Darkness> DarknessObjects = [];

    private static FsmInt value;

    private void OnEnable()
    {
        DarknessObjects.Add(this);
        Refresh();
    }

    private void OnDisable()
    {
        DarknessObjects.Remove(this);
        Refresh();
    }

    public static void Init()
    {
        On.PlayMakerFSM.Awake += (orig, self) =>
        {
            orig(self);
            if (self.FsmName == "Darkness Control") value = self.FsmVariables.FindFsmInt("Darkness Level");
        };
    }

    private static void Refresh()
    {
        GameManager.instance.sm.darknessLevel = Mathf.Min(DarknessObjects.Count, 2);
        value.Value = GameManager.instance.sm.darknessLevel;
        CustomObjects.RefreshLanternBinding();
    }
}