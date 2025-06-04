using System.Collections.Generic;
using Architect.objects;

namespace Architect.categories;

internal abstract class ObjectCategory
{
    internal string GetName()
    {
        return _name;
    }

    internal virtual int VerticalShift()
    {
        return 0;
    }

    internal abstract List<SelectableObject> GetObjects();

    private readonly string _name;

    protected ObjectCategory(string name)
    {
        _name = name;
    }
}