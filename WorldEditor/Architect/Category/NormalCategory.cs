using System.Collections.Generic;
using Architect.Objects;

namespace Architect.Category;

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

    internal void Sort()
    {
        _selectableObjects.Sort(Compare);

        if (GetName() == "Hazards") _selectableObjects.Reverse();
    }

    private static int Compare(SelectableObject obj1, SelectableObject obj2)
    {
        return obj1.GetWeight().CompareTo(obj2.GetWeight());
    }
}