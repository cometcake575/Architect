using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Relay : MonoBehaviour
{
    public bool canCall = true;
    
    public bool semiPersistent;
    public string id;
    
    private PersistentBoolItem _item;
    private bool _shouldRelay;

    public bool ShouldRelay()
    {
        return canCall && _shouldRelay;
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
