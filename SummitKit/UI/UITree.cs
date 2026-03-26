using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;
public static class UITree
{
    public static IEnumerable<IUIElement> DepthFirst(IUIElement root)
    {
        yield return root;

        foreach (var child in root.Children)
        {
            foreach (var node in DepthFirst(child))
                yield return node;
        }
    }
    public static IEnumerable<IUIElement> DepthFirstChildrenFirst(IUIElement root)
    {
        foreach (var child in root.Children)
        {
            foreach (var node in DepthFirstChildrenFirst(child))
                yield return node;
        }

        yield return root;
    }
    public static IUIElement? FindTopmost(IUIElement root, Point p)
    {
        if (!root.Visible) return null;

        foreach (var child in root.Children.Reverse<IUIElement>())
        {
            var hit = FindTopmost(child, p);
            if (hit != null) return hit;
        }

        return root.Layout.Contains(p) ? root : null;
    }

}