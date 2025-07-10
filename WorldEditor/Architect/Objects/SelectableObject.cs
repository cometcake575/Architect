using UnityEngine;

namespace Architect.Objects;

public abstract class SelectableObject
{
    private readonly string _name;
    
    public abstract void OnClickInWorld(Vector3 pos, bool first);

    public abstract bool IsFavourite();

    public abstract Sprite GetSprite();

    public abstract int GetWeight();

    public virtual float GetSpriteRotation()
    {
        return 0;
    }

    public virtual void AfterSelect()
    {
        
    }

    public string GetName()
    {
        return _name;
    }

    protected SelectableObject(string name)
    {
        _name = name;
    }
}