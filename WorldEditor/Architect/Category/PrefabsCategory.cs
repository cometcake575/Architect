using System.Collections.Generic;
using System.Linq;
using Architect.Objects;
using Architect.Storage;
using Architect.UI;

namespace Architect.Category;

internal class PrefabsCategory : ObjectCategory
{
    private static readonly List<SelectableObject> Prefabs = [];
    private static readonly List<ObjectPlacement> Objects = [];

    public PrefabsCategory() : base("Prefabs")
    {
        foreach (var obj in SceneSaveLoader.Load("Prefabs").Placements
                     .Where(obj => obj != null))
        {
            Prefabs.Add(new PrefabObject(obj));
            Objects.Add(obj);
        }
    }

    public static void TryAddPrefab()
    {
        if (EditorUIManager.SelectedItem is not PlaceableObject placeable) return;

        var placement = placeable.MakePlacement();

        Objects.Add(placement);
        SceneSaveLoader.Save("Prefabs", new LevelData(Objects, []));

        Prefabs.Add(new PrefabObject(placement));

        EditorUIManager.RefreshObjects();
        EditorUIManager.RefreshButtons();
    }

    public static void RemovePrefab(PrefabObject obj)
    {
        Prefabs.Remove(obj);
        Objects.RemoveAll(placement => placement.GetId() == obj.GetId());
        SceneSaveLoader.Save("Prefabs", new LevelData(Objects, []));

        EditorUIManager.RefreshObjects();
        EditorUIManager.RefreshButtons();
    }

    internal override List<SelectableObject> GetObjects()
    {
        return Prefabs;
    }
}