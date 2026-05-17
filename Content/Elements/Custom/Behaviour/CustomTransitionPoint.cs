using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomTransitionPoint : MonoBehaviour
{
    public int pointType;

    public static void Init()
    {
        On.TransitionPoint.GetGatePosition += (orig, self) =>
        {
            var ctp = self.GetComponent<CustomTransitionPoint>();
            return ctp ? ctp.GetGatePosition() : orig(self);
        };
    }

    public GatePosition GetGatePosition()
    {
        return pointType switch
        {
            0 => GatePosition.door,
            1 => GatePosition.left,
            2 => GatePosition.right,
            3 => GatePosition.top,
            _ => GatePosition.bottom
        };
    }
}