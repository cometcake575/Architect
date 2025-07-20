using System;
using System.Linq;
using Architect.Objects;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ObjectDuplicator : MonoBehaviour
{
    public string id;
    private ObjectPlacement _placement;

    public void Duplicate()
    {
        if (_placement == null)
        {
            _placement = PlacementManager.GetCurrentPlacements().FirstOrDefault(placement => placement.GetId() == id);
            if (_placement == null) return;
        }

        var obj = _placement.SpawnObject();
        obj.name += " Copy " + Guid.NewGuid();
        obj.SetActive(true);
    }
}