using UnityEngine;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Elements.Custom;

public class CustomTalkPlacer : MonoBehaviour
{
    public string triggerName;
    private CustomTalk[] _npcs;

    private void OnEnable()
    {
	if (_npcs != null && _npcs.Length > 0 && _npcs[0] != null) return;
        _npcs = RoomObjects.GetNPCs(this);
    }

    private void OnDisable()
    {
        foreach (var npc in _npcs ?? [])
        {
            if (npc) npc.HidePromptPublic(); // this method lives in CustomTalk.cs
        }
    }
}