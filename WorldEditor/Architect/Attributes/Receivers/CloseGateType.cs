namespace Architect.Attributes.Receivers;

public class CloseGateType : EventReceiverType
{
    public static readonly CloseGateType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new CloseGate();
    }

    public override string GetName()
    {
        return "close";
    }
}

internal class CloseGate : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        var fsm = instance.gameObject.LocateMyFSM("BG Control");
        fsm.SetState("Close 1");
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return CloseGateType.Instance;
    }
}