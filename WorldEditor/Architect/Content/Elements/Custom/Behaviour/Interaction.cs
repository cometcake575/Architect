using System.Collections.Generic;
using Architect.Attributes;
using Modding;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Interaction : MonoBehaviour
{
    private PromptMarker _prompt;

    public float xOffset;
    public float yOffset;
    public string prompt;
    public bool hideOnInteract;

    private static readonly List<Interaction> Interactable = [];

    public static void Init()
    {
        ModHooks.HeroUpdateHook += () =>
        {
            if (HeroController.instance.controlReqlinquished) return;
            if (!InputHandler.Instance.inputActions.up.WasPressed) return;
            var willRemove = new List<Interaction>();
            var willDown = new List<Interaction>();
            foreach (var interaction in Interactable)
            {
                if (!interaction)
                {
                    willRemove.Add(interaction);
                    continue;
                }

                if (HeroController.instance.transform.position.x > interaction.transform.position.x)
                {
                    HeroController.instance.FaceLeft();
                } else HeroController.instance.FaceRight();
                
                EventManager.BroadcastEvent(interaction.gameObject, "OnInteract");
                if (interaction.hideOnInteract) willDown.Add(interaction);
            }

            foreach (var interaction in willRemove) Interactable.Remove(interaction);
            foreach (var interaction in willDown) interaction.GoDown();
        };
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;
        GoUp();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;
        GoDown();
    }

    public void GoDown()
    {
        _prompt.Hide();
        Interactable.Remove(this);
    }

    public void GoUp()
    {
        var obj = Architect.ArrowPromptNew.Spawn();
        obj.transform.position = transform.position + new Vector3(xOffset, yOffset + 2);

        _prompt = obj.GetComponent<PromptMarker>();
        
        _prompt.SetLabel(prompt);
        _prompt.SetOwner(gameObject);
        _prompt.Show();
        
        Interactable.Add(this);
    }
}