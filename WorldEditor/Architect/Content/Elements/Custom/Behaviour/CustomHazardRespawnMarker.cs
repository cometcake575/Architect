namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomHazardRespawnMarker : HazardRespawnMarker
{
    private void Update()
    {
        respawnFacingRight = transform.GetScaleX() < 0;
    }
}