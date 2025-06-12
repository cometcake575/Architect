using System.Collections.Generic;
using System.Linq;
using Architect.Objects;

namespace Architect.Category;

internal class FavouritesCategory : ObjectCategory
{
    internal static readonly FavouritesCategory Instance = new();
    
    private FavouritesCategory() : base("Favourites")
    {
    }

    internal override List<SelectableObject> GetObjects()
    {
        return PlaceableObject.AllObjects.Values.ToList().FindAll(o => o.IsFavourite());
    }
}