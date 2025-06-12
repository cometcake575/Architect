using System.Collections.Generic;
using Architect.Objects;

namespace Architect.Category;

internal abstract class ObjectCategory
{
    internal string GetName()
    {
        return _name;
    }

    internal virtual bool CreateButton()
    {
        return true;
    }

    internal abstract List<SelectableObject> GetObjects();

    private readonly string _name;

    protected ObjectCategory(string name)
    {
        _name = name;
    }
}