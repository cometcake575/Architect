using Architect.Util;
using MagicUI.Core;
using MagicUI.Elements;

namespace Architect.UI;

public static class MenuUIManager
{
    public static void Initialize(LayoutRoot layout)
    {
        var img = new Image(layout, WeSpriteUtils.Load("architect_knight"))
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Width = 120,
            Height = 120,
            PreserveAspectRatio = true,
            Padding = new Padding(30, 30)
        };
        var find = new Button(layout, "Find Levels")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            MinWidth = 140,
            MinHeight = 140,
            FontSize = 28,
            Padding = new Padding(20, 20)
        };
    }
}