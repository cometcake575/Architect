using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Relay : MonoBehaviour
{
    public bool canCall = true;
    
    private void Update()
    {
        canCall = true;
    }
}