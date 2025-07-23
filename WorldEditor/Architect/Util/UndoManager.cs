using System.Collections.Generic;
using System.Linq;
using Architect.MultiplayerHook;
using Architect.Objects;
using Modding;
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

public class MoveObject(string placementId, Vector3 oldPos) : IUndoable
{
    public IUndoable Undo()
    {
        var obj = PlacementManager.GetCurrentPlacements().FirstOrDefault(pl => pl.GetId() == placementId);
        if (obj == null) return null;
        
        var newPos = obj.GetPos();
        obj.Move(oldPos);
        
        if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
        {
            HkmpHook.Update(placementId, GameManager.instance.sceneName, oldPos);
        }
        
        return new MoveObject(placementId, newPos);
    }
}