using System.Collections.Generic;
using Architect.Objects;

namespace Architect.Category;

internal abstract class ObjectCategory
{
    private readonly string _name;

    protected ObjectCategory(string name)
    {
        _name = name;
    }

    internal string GetName()
    {
        return _name;
    }

    internal virtual bool CreateButton()
    {
        return true;
    }

    internal abstract List<SelectableObject> GetObjects();
}