namespace Architect.Attributes.Receivers;

public class DieType : EventReceiverType
{
    public static readonly DieType Instance = new();

    protected override EventReceiver Instantiate()
    {
        return new Die();
    }

    public override string GetName()
    {
        return "die";
    }
}

internal class Die : EventReceiver
{
    public override void ReceiveEvent(EventReceiverInstance instance)
    {
        instance.gameObject.GetComponent<HealthManager>().Die(null, AttackTypes.Generic, true);
    }
        
    protected override EventReceiverType GetReceiverType()
    {
        return DieType.Instance;
    }
}