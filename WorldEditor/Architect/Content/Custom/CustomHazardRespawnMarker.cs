using System;

namespace Architect.Content.Custom;

public class CustomHazardRespawnMarker : HazardRespawnMarker
{
    private void Update()
    {
        respawnFacingRight = transform.GetScaleX() < 0;
    }
}