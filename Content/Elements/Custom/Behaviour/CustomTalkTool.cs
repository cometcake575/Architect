using System.Collections.Generic;
using UnityEngine;
using Satchel;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomTalk : MonoBehaviour
{
    public static CustomTalk currentTalkingNPC;
    public string[] dialoguePages;
    public void HidePromptPublic() => HidePrompt();
    private GameObject prompt;

    void Start()
    {
        var col = gameObject.GetComponent<BoxCollider2D>() 
                  ?? gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(3f, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HidePrompt();
    }

    void Update()
    {
        if (prompt?.activeSelf == true && Input.GetKeyDown(KeyCode.UpArrow))
            StartDialogue();
    }

    void ShowPrompt()
    {
	if (prompt == null)
    	{
        	prompt = Object.Instantiate(Architect.ArrowPromptNew);
        	prompt.transform.position = transform.position + new Vector3(0, 1.5f, 0);
    	}

    	prompt?.SetActive(true);
    }

    void HidePrompt() => prompt?.SetActive(false);

    void StartDialogue()
    {
        HidePrompt();
        currentTalkingNPC = this;
    	var box = GameCameras.instance.hudCamera.GetComponentInChildren<DialogueBox>();
    	box.StartConversation("CUSTOM_NPC", "CustomSheet");
    }
}