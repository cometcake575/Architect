using System.Collections.Generic;
using UnityEngine;
using Architect.Content;
using Architect.utils;

namespace Architect.objects;

public class PlaceableObject : SelectableObject
{
    public static readonly Dictionary<string, SelectableObject> AllObjects = new();
    
    public GameObject GetPrefab()
    {
        return _prefab;
    }
    
    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;
        new ObjectPlacement(GetName(), pos, Architect.IsFlipped, Architect.Rotation).Initialize();
    }

    public override bool IsFavourite()
    {
        return Architect.GlobalSettings.Favourites.Contains(GetName());
    }

    public override bool FlipX()
    {
        return _flipX;
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public void ToggleFavourite()
    {
        if (IsFavourite())
        {
            Architect.GlobalSettings.Favourites.Remove(GetName());
        }  else Architect.GlobalSettings.Favourites.Add(GetName());
    }

    private void PrepareSprite()
    {
        tk2dSpriteAnimator animator = GetPrefab().gameObject.GetComponentInChildren<tk2dSpriteAnimator>();
        if (animator)
        {
            tk2dSpriteAnimationFrame frame = animator.DefaultClip.frames[0];
            _sprite = WESpriteUtils.ConvertFrom2DToolkit(frame.spriteCollection.spriteDefinitions[frame.spriteId]);
            if (frame.spriteCollection.spriteDefinitions[frame.spriteId].flipped != tk2dSpriteDefinition.FlipMode.None) _rotation += 90;
            return;
        }
        tk2dSprite sprite = GetPrefab().gameObject.GetComponentInChildren<tk2dSprite>();
        if (sprite)
        {
            _sprite = WESpriteUtils.ConvertFrom2DToolkit(sprite.CurrentSprite);
            if (sprite.CurrentSprite.flipped != tk2dSpriteDefinition.FlipMode.None) _rotation += 90;
            return;
        }
        SpriteRenderer spriteRenderer = GetPrefab().gameObject.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) _sprite = spriteRenderer.sprite;
    }

    public override int GetRotation()
    {
        return _rotation;
    }
    
    private Sprite _sprite;
    private readonly GameObject _prefab;
    private int _rotation;
    private readonly bool _flipX;

    private PlaceableObject(ContentPack.AbstractPackElement element) : base(element.GetName())
    {
        _prefab = element.GetPrefab();
        _flipX = element.FlipX();
        PrepareSprite();
    }

    public static SelectableObject GetOrCreate(ContentPack.AbstractPackElement element)
    {
        if (AllObjects.TryGetValue(element.GetName(), out var obj)) return obj;
        PlaceableObject o = new PlaceableObject(element);
        AllObjects.Add(element.GetName(), o);
        return o;
    }
}