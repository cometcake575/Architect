using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class KeyListener : MonoBehaviour
{
    public int listenMode;
    public KeyCode key = KeyCode.Space;
    
    private void Update()
    {
        if (!(listenMode switch
            {
                0 => Input.GetKeyDown(key),
                1 => Input.GetKeyUp(key),
                _ => Input.GetKey(key)
            })) return;
        EventManager.BroadcastEvent(gameObject, "KeyPress");
    }
}