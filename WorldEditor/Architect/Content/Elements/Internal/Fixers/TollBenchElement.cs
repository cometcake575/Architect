using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class TollBenchElement : BenchElement
{
    public TollBenchElement(int weight) : base("Fungus3_50", "Toll Machine Bench", "Toll Bench", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.TollBench);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        base.PostSpawn(gameObject, flipped, rotation, scale);
        gameObject.transform.GetChild(2).name = gameObject.name + " Bench";
    }
}