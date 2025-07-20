using System;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class TextDisplay : MonoBehaviour
{
    public string ID { get; private set; }

    private PlayMakerFSM _dreamDialogue;

    private void Awake()
    {
        ID = gameObject.name;
    }

    private void Start()
    {
        _dreamDialogue = GameObject.Find("_GameCameras/HudCamera/DialogueManager/Dream Msg").LocateMyFSM("Display");
    }

    public void Display()
    {
        if (!isActiveAndEnabled) return;
        _dreamDialogue.FsmVariables.FindFsmString("Convo Title").Value = ID;
        _dreamDialogue.SendEvent("DISPLAY DREAM MSG ALT");
    }
}