namespace Architect.Attributes.Receivers;

public class DisableOnEventType : EventReceiverType
{
    public static readonly DisableOnEventType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new DisableOnEvent();
    }

    public override string GetName()
    {
        return "disable";
    }
}

internal class DisableOnEvent : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        instance.gameObject.SetActive(false);
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return DisableOnEventType.Instance;
    }
}