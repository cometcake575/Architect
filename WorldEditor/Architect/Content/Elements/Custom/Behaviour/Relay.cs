using Architect.Attributes;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Relay : MonoBehaviour
{
    public bool canCall = true;
    
    public bool semiPersistent;
    public string id;
    public float relayChance = 1;
    public bool multiplayerBroadcast;
    public float delay;
    
    private PersistentBoolItem _item;
    private bool _shouldRelay;
    private float _schedule = -1;

    public bool ShouldRelay()
    {
        if (Random.value > relayChance) return false;
        return canCall && _shouldRelay;
    }

    public void DoRelay()
    {
        if (!ShouldRelay()) return;
        canCall = false;
        if (delay <= 0) EventManager.BroadcastEvent(gameObject, "OnCall", multiplayerBroadcast);
        else _schedule = delay;
    }

    private void Awake()
    {
        if (id == null) return;

        gameObject.name = "[Architect] Relay (" + id + ")"; 
        
        _item = gameObject.AddComponent<PersistentBoolItem>();
        
        _item.persistentBoolData = new PersistentBoolData
        {
            id = id,
            sceneName = "Universal"
        };
        _item.enabled = true;

        _item.OnSetSaveState += value =>
        {
            _shouldRelay = !value;
        };

        _item.OnGetSaveState += (ref bool value) =>
        {
            value = !_shouldRelay;
        };

        _item.semiPersistent = semiPersistent;
        _item.persistentBoolData.semiPersistent = semiPersistent;

        _shouldRelay = true;
    }

    private void Update()
    {
        canCall = true;
        if (_schedule > 0)
        {
            _schedule -= Time.deltaTime;
            if (_schedule <= 0) EventManager.BroadcastEvent(gameObject, "OnCall", multiplayerBroadcast);
        }
    }

    public void EnableRelay()
    {
        _shouldRelay = true;
    }

    public void DisableRelay()
    {
        _shouldRelay = false;
    }
}
