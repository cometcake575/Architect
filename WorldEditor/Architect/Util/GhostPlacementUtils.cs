using Architect.Objects;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Util;

public static class GhostPlacementUtils
{
    public static void SetupForPlacement(GameObject obj, SpriteRenderer renderer, PlaceableObject selected, [CanBeNull] Sprite overrideSprite, bool flipped, float rotation, float scaleX, float scaleY)
    {
        renderer.sprite = overrideSprite ? overrideSprite : selected.GetSprite();

        obj.transform.localScale = new Vector2(scaleX, scaleY) * selected.Scale;
        
        var visualRotation = selected.GetSpriteRotation();
        
        obj.transform.rotation = Quaternion.Euler(0, 0, visualRotation + rotation);
        
        if (Mathf.RoundToInt(visualRotation / 90) % 2 != 0)
        {
            renderer.flipY = flipped != selected.PackElement.ShouldFlipHorizontal();
            renderer.flipX = selected.PackElement.ShouldFlipVertical();
        }
        else
        {
            renderer.flipX = flipped != selected.PackElement.ShouldFlipHorizontal();
            renderer.flipY = selected.PackElement.ShouldFlipVertical();
        }
    }
}