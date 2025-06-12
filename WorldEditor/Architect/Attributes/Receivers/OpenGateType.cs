using Satchel;

namespace Architect.Attributes.Receivers;

public class OpenGateType : EventReceiverType
{
    public static readonly OpenGateType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new OpenGate();
    }

    public override string GetName()
    {
        return "open";
    }
}

internal class OpenGate : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        foreach (var fsm in instance.gameObject.GetComponents<PlayMakerFSM>())
        {
            if (!fsm.TryGetState("Open", out var state)) continue;
            fsm.SetState(state.Name);
        }
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return OpenGateType.Instance;
    }
}