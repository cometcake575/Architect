using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Attributes.Receivers;

public class EventReceiverInstance : MonoBehaviour
{
    private int _calls;
    
    public void ReceiveEvent()
    {
        _calls++;
        if (_calls < Receiver?.RequiredCalls) return;
        if (Receiver != null) EventManager.RunEvent(Receiver.TypeName, gameObject);
        _calls = 0;
    }

    [CanBeNull] public EventReceiver Receiver;
}