using UnityEngine;

namespace Architect.objects;

public abstract class SelectableObject
{
    private readonly string _name;
    
    public abstract void OnClickInWorld(Vector3 pos, bool first);

    public abstract bool IsFavourite();

    public virtual bool FlipX()
    {
        return false;
    }

    public abstract Sprite GetSprite();

    public virtual int GetRotation()
    {
        return 0;
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