using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Receivers;
using JetBrains.Annotations;

namespace Architect.Content.Groups;

public class ReceiverGroup
{
    private static bool _initialized;
    
    internal static ReceiverGroup All;
    
    internal static ReceiverGroup Gates;
    
    internal static ReceiverGroup BattleGate;
    
    internal static ReceiverGroup Enemies;
    
    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        All = new ReceiverGroup(null, 
            EventManager.RegisterEventReceiverType(DisableOnEventType.Instance),
            EventManager.RegisterEventReceiverType(EnableOnEventType.Instance)
        );
        
        Gates = new ReceiverGroup(All, EventManager.RegisterEventReceiverType(OpenGateType.Instance));
        
        BattleGate = new ReceiverGroup(Gates, EventManager.RegisterEventReceiverType(CloseGateType.Instance));

        Enemies = new ReceiverGroup(All, EventManager.RegisterEventReceiverType(DieType.Instance));
    }

    public readonly EventReceiverType[] Types;
    
    private ReceiverGroup([CanBeNull] ReceiverGroup parent, params EventReceiverType[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}