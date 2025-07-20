#nullable enable
using Architect.Content.Groups;
using Architect.Objects;
using UnityEngine;

namespace Architect.Content.Elements;

public abstract class AbstractPackElement
{
    public abstract GameObject GetPrefab(bool flipped, float rotation);

    private readonly string _name;
    private readonly string _category;
        
    private bool _flipVertical;
    private bool _flipHorizontal;
    private string[] _broadcasterGroup = BroadcasterGroup.EmptyGroup;
    private ReceiverGroup _receiverGroup = ReceiverGroup.All;
    private ConfigGroup _configGroup = ConfigGroup.Generic;
    private RotationGroup _rotationGroup = RotationGroup.None;

    internal readonly int Weight;

    public virtual Sprite? GetSprite()
    {
        return null;
    }
    
    internal string[] GetBroadcasterGroup()
    {
        return _broadcasterGroup;
    }

    internal ReceiverGroup GetReceiverGroup()
    {
        return _receiverGroup;
    }

    internal ConfigGroup GetConfigGroup()
    {
        return _configGroup;
    }

    internal RotationGroup GetRotationGroup()
    {
        return _rotationGroup;
    }

    public AbstractPackElement WithBroadcasterGroup(string[] group)
    {
        _broadcasterGroup = group;
        return this;
    }

    public AbstractPackElement WithReceiverGroup(ReceiverGroup group)
    {
        _receiverGroup = group;
        return this;
    }

    public AbstractPackElement WithConfigGroup(ConfigGroup group)
    {
        _configGroup = group;
        return this;
    }

    public AbstractPackElement WithRotationGroup(RotationGroup group)
    {
        _rotationGroup = group;
        return this;
    }

    internal string GetName()
    {
        return _name;
    }

    internal string GetCategory()
    {
        return _category;
    }

    protected AbstractPackElement(string name, string category, int weight)
    {
        _name = name;
        _category = category;
        Weight = weight;
    }

    public virtual bool OverrideFlip()
    {
        return false;
    }

    public virtual bool OverrideRotation()
    {
        return false;
    }

    public bool ShouldFlipVertical()
    {
        return _flipVertical;
    }

    public bool ShouldFlipHorizontal()
    {
        return _flipHorizontal;
    }

    public AbstractPackElement FlipVertical()
    {
        _flipVertical = true;
        return this;
    }

    public AbstractPackElement FlipHorizontal()
    {
        _flipHorizontal = true;
        return this;
    }

    public virtual bool DisableScaleParent()
    {
        return false;
    }

    // After the object is spawned in
    public virtual void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale) { }
    
    // After the preview is spawned in
    public virtual void PostPlace(ObjectPlacement placement, GameObject preview) { }
}