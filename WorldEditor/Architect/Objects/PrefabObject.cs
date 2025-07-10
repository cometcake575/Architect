using Architect.Category;

namespace Architect.Objects;

public class PrefabObject : PlaceableObject
{
    private readonly ObjectPlacement _placement;
    
    public PrefabObject(ObjectPlacement placement) : base(placement.GetPlaceableObject().PackElement)
    {
        _placement = placement;
    }

    public override void AfterSelect()
    {
        PickObject.LoadConfig(_placement);
    }

    public string GetId()
    {
        return _placement.GetId();
    }

    public void Delete()
    {
        PrefabsCategory.RemovePrefab(this);
    }
}