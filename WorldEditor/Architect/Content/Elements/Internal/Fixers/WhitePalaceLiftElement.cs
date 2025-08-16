using System.Collections;
using System.Collections.Generic;
using Architect.Content.Groups;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class WhitePalaceLiftElement : InternalPackElement
{
    private GameObject _gameObject;

    public WhitePalaceLiftElement(int weight) : base("White Palace Lift", "Interactable", weight:weight)
    {
        WithConfigGroup(ConfigGroup.PalaceLift);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("White_Palace_03_hub", "White Palace Lift"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["White_Palace_03_hub"]["White Palace Lift"];
        _gameObject.AddComponent<WpLiftConfig>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Control");
        var config = gameObject.GetComponent<WpLiftConfig>();

        var rise = fsm.GetAction<Translate>("Rise", 4);
        var setVel = fsm.GetAction<SetVelocity2d>("Rise", 5);

        var move = new Vector2(config.xMove, config.yMove).normalized * 30;
        
        rise.x = move.x;
        rise.y = move.y;
        setVel.vector = move;

        var endX = gameObject.transform.position.x + config.xMove;
        var endY = gameObject.transform.position.y + config.yMove;
        
        fsm.DisableAction("Rise", 7);

        fsm.InsertCustomAction("Rise", makerFsm =>
        {
            config.StartCoroutine(Rise(config, makerFsm, endX, endY));
        }, 7);

        var sp = fsm.GetAction<SetPosition>("Hit Top", 0);
        sp.x = endX;
        sp.y = endY;

        fsm.GetAction<SetPosition>("Return", 1).x = gameObject.transform.position.x;
    }

    private static IEnumerator Rise(WpLiftConfig config, PlayMakerFSM makerFsm, float endX, float endY)
    {
        var yMoved = false;
        var xMoved = false;

        while (!xMoved || !yMoved)
        {
            var pos = makerFsm.transform.position;
            
            yMoved = config.yMove == 0 ||
                     (config.yMove > 0 && pos.y >= endY) ||
                     (config.yMove < 0 && pos.y <= endY);

            xMoved = config.xMove == 0 ||
                     (config.xMove > 0 && pos.x >= endX) ||
                     (config.xMove < 0 && pos.x <= endX);

            yield return null;
        }
        makerFsm.SendEvent("TOP");
    }
}

public class WpLiftConfig : MonoBehaviour
{
    public float xMove;
    public float yMove;
}