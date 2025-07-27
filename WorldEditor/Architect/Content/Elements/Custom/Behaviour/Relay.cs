using Architect.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Relay : MonoBehaviour
{
    public bool canCall = true;

    public bool startActivated = true;
    public bool semiPersistent;
    [CanBeNull] public string id;
    public float relayChance = 1;
    public bool multiplayerBroadcast;
    public float delay;
    
    private PersistentRelayItem _item;
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
        _shouldRelay = startActivated;
        if (string.IsNullOrEmpty(id)) return;
        
        _item = gameObject.AddComponent<PersistentRelayItem>();
        
        _item.persistentRelayData = new PersistentIntData
        {
            id = id,
            sceneName = "Universal",
            value = startActivated ? 1 : 0
        };

        _item.OnSetSaveState += value =>
        {
            if (value == -1) return;
            _shouldRelay = value == 1;
        };

        _item.OnGetSaveState += (ref int value) =>
        {
            value = _shouldRelay ? 1 : 0;
        };

        _item.semiPersistent = semiPersistent;
        _item.persistentRelayData.semiPersistent = semiPersistent;
        
        _item.enabled = true;
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

public class PersistentRelayItem : MonoBehaviour
{
  [SerializeField] public bool semiPersistent;
  [SerializeField] public PersistentIntData persistentRelayData;
  private GameManager _gm;

  public event IntEvent OnSetSaveState;

  public event IntRefEvent OnGetSaveState;

  private void OnEnable()
  {
    _gm = GameManager.instance;
    _gm.SavePersistentObjects += SaveState;
    if (semiPersistent) _gm.ResetSemiPersistentObjects += ResetState;
  }

  private void OnDisable()
  {
    _gm.SavePersistentObjects -= SaveState;
    _gm.ResetSemiPersistentObjects -= ResetState;
  }

  private void Start()
  {
    var myState = SceneData.instance.FindMyState(persistentRelayData);
    if (myState != null)
    { 
        persistentRelayData.value = myState.value; 
        OnSetSaveState?.Invoke(myState.value);
    }
    else OnGetSaveState?.Invoke(ref persistentRelayData.value);
  }

  public void SaveState()
  {
    OnGetSaveState?.Invoke(ref persistentRelayData.value);
    SceneData.instance.SaveMyState(persistentRelayData);
  }

  private void ResetState()
  {
    if (!semiPersistent) return;
    persistentRelayData.value = -1;
  }

  public void PreSetup() => Start();

  public delegate void IntEvent(int value);

  public delegate void IntRefEvent(ref int value);
}
