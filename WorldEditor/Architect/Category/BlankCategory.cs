using System.Collections.Generic;
using Architect.Objects;

namespace Architect.Category;

internal class BlankCategory : ObjectCategory
{
    public BlankCategory() : base("")
    {
    }

    internal override List<SelectableObject> GetObjects()
    {
        return new List<SelectableObject>();
    }

    internal override bool CreateButton()
    {
        return false;
    }
}