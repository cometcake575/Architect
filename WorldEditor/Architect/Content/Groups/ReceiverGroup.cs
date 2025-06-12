using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Receivers;
using JetBrains.Annotations;

namespace Architect.Content.Groups;

public class ReceiverGroup
{
    internal static ReceiverGroup All;
    internal static ReceiverGroup Gates;
    
    internal static void Initialize()
    {
        All = new ReceiverGroup(null, 
            EventManager.RegisterEventReceiverType(DisableOnEventType.Instance),
            EventManager.RegisterEventReceiverType(EnableOnEventType.Instance)
        );
        
        Gates = new ReceiverGroup(All, EventManager.RegisterEventReceiverType(OpenGateType.Instance));
    }

    public readonly EventReceiverType[] Types;
    
    private ReceiverGroup([CanBeNull] ReceiverGroup parent, params EventReceiverType[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}