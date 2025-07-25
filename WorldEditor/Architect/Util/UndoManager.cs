using System.Collections.Generic;
using System.Linq;
using Architect.MultiplayerHook;
using Architect.Objects;
using Modding;
using Mono.Collections.Generic;
using UnityEngine;

namespace Architect.Util;

public static class UndoManager
{
    private static readonly List<IUndoable> Before = [];
    private static readonly List<IUndoable> After = [];

    private static string _lastScene;

    public static void Initialize()
    {
        On.GameManager.LoadScene += (orig, self, scene) =>
        {
            if (_lastScene != scene)
            {
                _lastScene = scene;
                Before.Clear();
                After.Clear();
            }

            orig(self, scene);
        };
    }

    public static void UndoLast()
    {
        if (Before.Count == 0) return;
        
        var result = Before[Before.Count - 1].Undo();
        if (result != null) After.Add(result);
        else Before.Clear();
        
        Before.RemoveAt(Before.Count - 1);
    }

    public static void RedoLast()
    {
        if (After.Count == 0) return;
        
        var result = After[After.Count - 1].Undo();
        if (result != null) Before.Add(result);
        else After.Clear();
        
        After.RemoveAt(After.Count - 1);
    }
    
    public static void PerformAction(IUndoable undoable) {
        After.Clear();
        Before.Add(undoable);
    }
}

public interface IUndoable
{
    IUndoable Undo();
}

public class PlaceObject(string placementId) : IUndoable
{
    public IUndoable Undo()
    {
        var obj = PlacementManager.GetCurrentPlacements().FirstOrDefault(pl => pl.GetId() == placementId);
        if (obj == null) return null;

        if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
        {
            HkmpHook.Erase(placementId, GameManager.instance.sceneName);
        }
        
        obj.Destroy();
        return new EraseObject(obj);
    }
}

public class EraseObject(ObjectPlacement placement) : IUndoable
{
    public IUndoable Undo()
    {
        PlacementManager.GetCurrentPlacements().Add(placement);
        placement.PlaceGhost();

        if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
        {
            HkmpHook.Place(placement, GameManager.instance.sceneName);
        }
        
        return new PlaceObject(placement.GetId());
    }
}

public class MoveObjects(List<(string, Vector3)> data) : IUndoable
{
    public IUndoable Undo()
    {
        List<(string, Vector3)> reversed = [];
        Architect.Instance.Log(data.Count());
        foreach (var pair in data)
        {
            var obj = PlacementManager.GetCurrentPlacements().FirstOrDefault(pl => pl.GetId() == pair.Item1);
            if (obj == null) return null;

            reversed.Add((pair.Item1, obj.GetPos()));
            
            obj.Move(pair.Item2);
            obj.StoreOldPos();

            if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
            {
                HkmpHook.Update(pair.Item1, GameManager.instance.sceneName, pair.Item2);
            }
        }
        return new MoveObjects(reversed);
    }
}