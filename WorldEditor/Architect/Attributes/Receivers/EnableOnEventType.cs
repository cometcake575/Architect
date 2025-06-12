namespace Architect.Attributes.Receivers;

public class EnableOnEventType : EventReceiverType
{
    public static readonly EnableOnEventType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new EnableOnEvent();
    }

    public override string GetName()
    {
        return "enable";
    }
}

internal class EnableOnEvent : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        instance.gameObject.SetActive(true);
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return EnableOnEventType.Instance;
    }
}