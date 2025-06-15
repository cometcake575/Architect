using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using UnityEngine;
using Architect.Content.Elements;
using Architect.UI;
using Architect.Util;

namespace Architect.Objects;

public class PlaceableObject : SelectableObject
{
    public static readonly Dictionary<string, SelectableObject> AllObjects = new();
    
    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;
        pos.z = GetZPos();

        var broadcasters = EditorUIManager.Broadcasters.ToArray();
        var receivers = EditorUIManager.Receivers.ToArray();
        var config = EditorUIManager.ConfigValues.Values.ToArray();
        
        var placement = new ObjectPlacement(
            GetName(),
            pos, 
            EditorManager.IsFlipped, 
            EditorManager.Rotation, 
            EditorManager.Scale, 
            Guid.NewGuid().ToString().Substring(0, 8),
            broadcasters,
            receivers,
            config
        );
        
        PlacementManager.GetCurrentPlacements().Add(placement);
        placement.PlaceGhost();
    }

    public override bool IsFavourite()
    {
        return Architect.GlobalSettings.Favourites.Contains(GetName());
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public override int GetWeight()
    {
        return _weight;
    }

    public float GetZPos()
    {
        return _zPosition;
    }

    public void ToggleFavourite()
    {
        if (IsFavourite())
        {
            Architect.GlobalSettings.Favourites.Remove(GetName());
        }  else Architect.GlobalSettings.Favourites.Add(GetName());
    }

    private void PreparePlacementData(GameObject prefab)
    {
        if (PackElement.GetSprite())
        {
            _sprite = PackElement.GetSprite();
            return;
        }
        
        _zPosition = prefab.transform.position.z;
        
        var spriteRenderer = prefab.gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            _sprite = spriteRenderer.sprite;
            return;
        }
        var sprite = prefab.gameObject.GetComponent<tk2dSprite>();
        if (sprite)
        {
            PrepareSpriteWithTk2D(sprite);
            return;
        }
        
        var cSprite = prefab.gameObject.GetComponentInChildren<tk2dSprite>();
        if (cSprite)
        {
            PrepareSpriteWithTk2D(cSprite);
            Offset += prefab.transform.InverseTransformPoint(cSprite.gameObject.transform.position);
            return;
        }
        var cSpriteRenderer = prefab.gameObject.GetComponentInChildren<SpriteRenderer>();
        if (!cSpriteRenderer) return;
        
        _sprite = cSpriteRenderer.sprite;
        Offset += prefab.transform.InverseTransformPoint(cSpriteRenderer.gameObject.transform.position);
    }

    private void PrepareSpriteWithTk2D(tk2dSprite sprite)
    {
        var animator = sprite.gameObject.GetComponent<tk2dSpriteAnimator>();
        tk2dSpriteDefinition def;

        if (animator)
        {
            var frame = animator.DefaultClip.frames[0];
            def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
        }
        else def = sprite.CurrentSprite;

        _sprite = WeSpriteUtils.ConvertFrom2DToolkit(def, 64 / sprite.scale.x);
        
        if (def.flipped != tk2dSpriteDefinition.FlipMode.None) _rotation += 90;
        Offset = def.GetBounds().center;
    }

    public override int GetSpriteRotation()
    {
        return _rotation;
    }
    
    private Sprite _sprite;
    private int _rotation;
    public readonly AbstractPackElement PackElement;

    public Vector3 Offset;
    private float _zPosition;
    private readonly int _weight;

    private static int _nextWeight;

    private PlaceableObject(AbstractPackElement element) : base(element.GetName())
    {
        _weight = element.Weight * -1000 + _nextWeight;
        _nextWeight++;
        PackElement = element;

        var prefab = element.GetPrefab(false, 0);
        PreparePlacementData(prefab);
        Offset = prefab.transform.rotation * Offset;
    }

    public static SelectableObject Create(AbstractPackElement element)
    {
        if (AllObjects.TryGetValue(element.GetName(), out var obj)) return obj;
        var o = new PlaceableObject(element);
        AllObjects.Add(element.GetName(), o);
        return o;
    }
}