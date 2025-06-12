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
        Receiver?.ReceiveEvent(this);
        _calls = 0;
    }

    [CanBeNull] public EventReceiver Receiver;
}