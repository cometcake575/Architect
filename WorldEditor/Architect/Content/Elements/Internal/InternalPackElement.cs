using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Internal;

internal abstract class InternalPackElement : AbstractPackElement
{
    protected InternalPackElement(string name, string category, int weight = 0) : base(name, category, weight)
    {
    }

    internal abstract void AddPreloads(List<(string, string)> preloadInfo);

    internal abstract void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads);
}