namespace Architect.Content.Elements.Custom;

public class CustomHazardRespawnMarker : HazardRespawnMarker
{
    private void Update()
    {
        respawnFacingRight = transform.GetScaleX() < 0;
    }
}