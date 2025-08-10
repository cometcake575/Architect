using System.Collections.Generic;
using System.Linq;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ConveyorBeltElement : InternalPackElement
{
    private GameObject _gameObject;

    public ConveyorBeltElement(int weight) : base("Crystal Peak Conveyor Belt", "Interactable", weight:weight)
    {
        WithRotationGroup(RotationGroup.Three);
        WithConfigGroup(ConfigGroup.Conveyors);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Mines_31", "Conveyor Block"));
        preloadInfo.Add(("Mines_31", "conveyor_belt_0mid (3)/conveyor_belt_simple0004"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        Initialize();
        
        _gameObject = preloads["Mines_31"]["conveyor_belt_0mid (3)/conveyor_belt_simple0004"];
        
        var col = preloads["Mines_31"]["Conveyor Block"];
        col.transform.parent = _gameObject.transform;

        var scale = col.transform.localScale;
        scale.x = 5.1f;
        scale.y = 0.5f;
        scale.z = 1;
        col.transform.localScale = scale;
        
        var colPos = col.transform.localPosition;
        colPos.x = 0;
        colPos.y = -0.0703f;
        colPos.z = 0;
        col.transform.localPosition = colPos;
        
        var pos = _gameObject.transform.position;
        pos.z = 0;
        _gameObject.transform.position = pos;
        
        col.SetActive(true);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var belt = gameObject.transform.GetChild(0).gameObject.GetComponent<ConveyorBelt>();
        if (flipped == rotation is 270) belt.speed = -belt.speed;
        if (rotation is 90 or 270) belt.vertical = true;
    }

    public static void Initialize()
    {
        On.HeroController.SceneInit += (orig, self) =>
        {
            orig(self);
            CurrentVerticalBelts.Clear();
            CurrentBelts.Clear();
            self.GetComponent<ConveyorMovementHero>().StopConveyorMove();
            HeroController.instance.cState.onConveyor = false;
            HeroController.instance.cState.onConveyorV = false;
        };
        
        On.ConveyorBelt.OnCollisionExit2D += (orig, self, collision) =>
        {
            if (self.vertical)
            {
                if (collision.gameObject.GetComponent<HeroController>())
                {
                    CurrentVerticalBelts.Remove(self);
                    if (CurrentVerticalBelts.Count > 0) return;
                }
            }
            else
            {
                var move = collision.gameObject.GetComponent<ConveyorMovement>();
                if (move)
                {
                    if (CurrentBelts.TryGetValue(move, out var group))
                    {
                        group.Remove(self);
                        if (group.Count > 0) return;
                    }
                }
            }

            orig(self, collision);
        };
        
        On.ConveyorBelt.OnCollisionEnter2D += (orig, self, collision) =>
        {
            orig(self, collision);
            if (self.vertical)
            {
                if (collision.gameObject.GetComponent<HeroController>()) CurrentVerticalBelts.Add(self);
            }
            else
            {
                var move = collision.gameObject.GetComponent<ConveyorMovement>();
                if (move)
                {
                    if (!CurrentBelts.Keys.Contains(move)) CurrentBelts[move] = [self];
                    else CurrentBelts[move].Add(self);
                }
            }
        };
    }

    private static readonly Dictionary<ConveyorMovement, List<ConveyorBelt>> CurrentBelts = [];
    private static readonly List<ConveyorBelt> CurrentVerticalBelts = [];
}
