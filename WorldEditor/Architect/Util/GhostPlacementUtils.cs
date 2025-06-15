using Architect.Objects;
using UnityEngine;

namespace Architect.Util;

public static class GhostPlacementUtils
{
    public static void SetupForPlacement(GameObject obj, SpriteRenderer renderer, PlaceableObject selected, bool flipped, int rotation, float scale)
    {
        renderer.sprite = selected.GetSprite();

        var prefab = selected.PackElement.GetPrefab(flipped, rotation);
        obj.transform.localScale = prefab.transform.localScale * scale;
        
        var visualRotation = selected.GetSpriteRotation();
        
        obj.transform.rotation = Quaternion.Euler(0, 0, visualRotation + rotation + prefab.transform.eulerAngles.z);

        if (visualRotation % 180 != 0)
        {
            renderer.flipY = flipped;
            renderer.flipX = selected.PackElement.ShouldFlipVertical();
        }
        else
        {
            renderer.flipX = flipped;
            renderer.flipY = selected.PackElement.ShouldFlipVertical();
        }
    }
}