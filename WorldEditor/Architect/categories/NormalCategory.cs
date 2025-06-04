using System.Collections.Generic;
using Architect.objects;

namespace Architect.categories;

internal class NormalCategory : ObjectCategory
{
    private readonly List<SelectableObject> _selectableObjects;
    
    internal NormalCategory(string name) : base(name)
    {
        _selectableObjects = new List<SelectableObject>();
    }

    internal void AddObject(SelectableObject obj)
    {
        _selectableObjects.Add(obj);
    }

    internal override List<SelectableObject> GetObjects()
    {
        return _selectableObjects;
    }
}