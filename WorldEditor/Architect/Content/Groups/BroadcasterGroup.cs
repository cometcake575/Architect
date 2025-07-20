namespace Architect.Content.Groups;

public class BroadcasterGroup
{
    public static readonly string[] EmptyGroup = [];

    public static readonly string[] Enemies = ["OnDeath", "OnDamage", "LoadedDead"];

    public static readonly string[] Levers = ["OnPull"];
    
    public static readonly string[] PersistentLevers = ["OnPull", "LoadedPulled"];
    
    public static readonly string[] TriggerZones = ["ZoneEnter", "ZoneExit"];

    public static readonly string[] Bindings = ["OnBind", "OnUnbind"];

    public static readonly string[] Callable = ["OnCall"];

    public static readonly string[] Tolls = ["OnPay", "LoadedPaid"];

    public static readonly string[] KeyListeners = ["KeyPress"];
    
    public static readonly string[] ZoteHead = ["Land", "InAir", "OnHit"];
    
    public static readonly string[] PlayerHook = [
        "OnDamage", 
        "OnHeal",
        "OnHazardRespawn", 
        "OnDeath",
        "FaceLeft",
        "FaceRight",
        "Jump", 
        "WallJump",
        "DoubleJump",
        "Land",
        "HardLand",
        "Dash",
        "CrystalDash",
        "Attack",
        "Spirit",
        "Dive",
        "Wraiths"
    ];
}