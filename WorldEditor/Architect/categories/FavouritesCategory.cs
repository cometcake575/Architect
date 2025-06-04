using System.Collections.Generic;
using System.Linq;
using Architect.objects;

namespace Architect.categories;

internal class FavouritesCategory : ObjectCategory
{
    internal static readonly FavouritesCategory Instance = new();
    
    private FavouritesCategory() : base("Favourites")
    {
    }

    internal override int VerticalShift()
    {
        return 20;
    }

    internal override List<SelectableObject> GetObjects()
    {
        return PlaceableObject.AllObjects.Values.ToList().FindAll(o => o.IsFavourite());
    }
}