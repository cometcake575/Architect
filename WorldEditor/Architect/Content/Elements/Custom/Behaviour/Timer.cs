using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Timer : MonoBehaviour
{
    private float _time;
    private int _calls;

    public float startDelay = 1;
    public float repeatDelay = 1;
    public int maxCalls = -1;
    
    private void Update()
    {
        if (startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            if (startDelay > 0) return;
            _time -= startDelay;
        } else
        {
            _time += Time.deltaTime;
            if (_time < repeatDelay) return;
            _time -= repeatDelay;
        }
        
        _calls++;
        EventManager.BroadcastEvent(gameObject, EventBroadcasterType.OnCall);
        if (maxCalls != -1 && _calls >= maxCalls)
        {
            _calls = 0;
            gameObject.SetActive(false);
        }
    }
}