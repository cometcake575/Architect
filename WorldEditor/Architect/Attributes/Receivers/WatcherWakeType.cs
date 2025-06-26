namespace Architect.Attributes.Receivers;

public class WatcherWakeType : EventReceiverType
{
    public static readonly WatcherWakeType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new WatcherWake();
    }

    public override string GetName()
    {
        return "wake";
    }
}

internal class WatcherWake : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        instance.gameObject.LocateMyFSM("Black Knight").SendEvent("WAKE");
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return WatcherWakeType.Instance;
    }
}