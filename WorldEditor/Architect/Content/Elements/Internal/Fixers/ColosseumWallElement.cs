using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ColosseumWallElement : InternalPackElement
{
    private GameObject _gameObject;

    public ColosseumWallElement(int weight) : base("Moving Colosseum Wall", "Interactable", weight:weight)
    {
        WithConfigGroup(ConfigGroup.MovingWall);
        WithReceiverGroup(ReceiverGroup.MovingWall);
        WithRotationGroup(RotationGroup.Four);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Room_Colosseum_Gold", "Colosseum Manager/Walls/Colosseum Wall L"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Room_Colosseum_Gold"]["Colosseum Manager/Walls/Colosseum Wall L"];
        var managedChild = _gameObject.transform.GetChild(0);
        
        managedChild.GetChild(0).gameObject.SetActive(false);
        foreach (var i in new []{0, 2, 4, 5, 6}) managedChild.GetChild(1).GetChild(i).gameObject.SetActive(false);

        var col = managedChild.GetComponent<BoxCollider2D>();
        col.size = new Vector2(1.4581f, 6.4249f);
        col.offset = Vector2.zero;

        _gameObject.AddComponent<CustomWallMover>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Control");

        var val = new Vector2(gameObject.GetComponent<CustomWallMover>().moveSpeed, 0);
        if (!flipped) val.x = -val.x;
        val = Quaternion.Euler(0, 0, rotation) * val;

        fsm.FsmVariables.FindFsmVector2("In Speed").Value = val;
        fsm.FsmVariables.FindFsmVector2("Out Speed").Value = -val;
    }
}

internal class CustomWallMover : MonoBehaviour
{
    public float moveDistance;
    public float moveSpeed = 28;
}
