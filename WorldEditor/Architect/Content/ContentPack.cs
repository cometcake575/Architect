using System.Collections.Generic;
using Architect.Content.Elements;

namespace Architect.Content;

public class ContentPack : List<AbstractPackElement>
{
    private readonly string _name;
    private readonly string _description;

    public ContentPack(string name, string description, bool enabledByDefault = true)
    {
        _name = name;
        _description = description;
        if (!Architect.GlobalSettings.ContentPackSettings.ContainsKey(name))
        {
            Architect.GlobalSettings.ContentPackSettings[name] = enabledByDefault;
        }
    }

    public string GetName()
    {
        return _name;
    }

    public string GetDescription()
    {
        return _description;
    }

    internal bool IsEnabled()
    {
        return Architect.GlobalSettings.ContentPackSettings[_name];
    }
}