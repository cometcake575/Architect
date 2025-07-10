using System.Collections.Generic;
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
        List<SelectableObject> favourites = [];
        foreach (var favourite in Architect.GlobalSettings.Favourites)
        {
            if (PlaceableObject.AllObjects.TryGetValue(favourite, out var o)) favourites.Add(o);
        }
        return favourites;
    }
}