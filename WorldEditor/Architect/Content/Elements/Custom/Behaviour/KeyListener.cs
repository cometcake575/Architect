using Architect.Attributes;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class KeyListener : MonoBehaviour
{
    public int listenMode;
    public KeyCode key = KeyCode.Space;

    private float _keyHeld;
    
    private void Update()
    {
        switch (listenMode)
        {
            case 0:
                if (!Input.GetKeyDown(key)) return;
                EventManager.BroadcastEvent(gameObject, "KeyPress");
                break;
            case 1:
                if (!Input.GetKeyUp(key)) return;
                EventManager.BroadcastEvent(gameObject, "KeyPress");
                break;
            default:
                if (!Input.GetKey(key)) return;
                _keyHeld += Time.deltaTime;
                while (_keyHeld > 0.01f)
                {
                    EventManager.BroadcastEvent(gameObject, "KeyPress");
                    _keyHeld -= 0.01f;
                }
                break;
        }
    }
}