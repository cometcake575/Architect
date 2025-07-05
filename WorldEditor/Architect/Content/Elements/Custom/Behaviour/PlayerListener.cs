using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class PlayerListener : MonoBehaviour
{
    private void OnEnable()
    { 
        CustomObjects.PlayerListeners.Add(this);
    }
    
    private void OnDisable()
    {
        CustomObjects.PlayerListeners.Remove(this);
    }
}