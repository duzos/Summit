using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;

public interface IUIElement : IDraw, IUpdating, IPositioned
{
    public static bool Debug = false;
    public static int Thickness = 2;

    bool Visible { get; set; }
    Rectangle Layout { get; set; }
    IUIElement? Parent { get; set; }
    Rectangle PreferredLayout { get; }
    int Padding { get; set; }
    int Spacing { get; set; }
    UIAlign HorizontalAlign { get; set; }
    UIAlign VerticalAlign { get; set; }
    bool ForceNewRow { get; set; }

    List<IUIElement> Children { get; }

    void AddChild(IUIElement child)
    {
        if (!Children.Contains(child))
        {
            Children.Add(child);
            child.Parent = this;

            RecalculateLayout();
        }
    }

    void RecalculateLayout()
    {
        int startX = Layout.X + Padding;
        int maxWidth = Layout.Width - Padding * 2;

        int cursorX = startX;
        int cursorY = Layout.Y + Padding;
        int rowHeight = 0;

        List<IUIElement> row = [];

        void LayoutRow()
        {
            int rowWidth = row.Sum(e => e.PreferredLayout.Width) +
                           Spacing * (row.Count - 1);

            int offsetX = 0;

            if (row.Count > 0 && rowWidth < maxWidth)
            {
                if (row.All(e => e.HorizontalAlign == UIAlign.Center))
                    offsetX = (maxWidth - rowWidth) / 2;
                else if (row.All(e => e.HorizontalAlign == UIAlign.End))
                    offsetX = maxWidth - rowWidth;
            }

            int x = startX + offsetX;

            foreach (var e in row)
            {
                int y = cursorY;

                if (e.VerticalAlign == UIAlign.Center)
                    y += (rowHeight - e.PreferredLayout.Height) / 2;
                else if (e.VerticalAlign == UIAlign.End)
                    y += rowHeight - e.PreferredLayout.Height;

                e.Layout = new Rectangle(
                    x,
                    y,
                    e.PreferredLayout.Width,
                    e.PreferredLayout.Height
                );
                e.LayerDepth = LayerDepth + 0.025F;

                x += e.PreferredLayout.Width + Spacing;
            }

            cursorY += rowHeight + Spacing;
            row.Clear();
            rowHeight = 0;
        }

        foreach (var child in Children)
        {
            int w = child.PreferredLayout.Width;
            int h = child.PreferredLayout.Height;

            if (cursorX + w > startX + maxWidth || child.ForceNewRow)
            {
                LayoutRow();
                cursorX = startX;
            }

            row.Add(child);

            child.RecalculateLayout();

            cursorX += w + Spacing;
            rowHeight = Math.Max(rowHeight, h);
        }

        if (row.Count > 0)
            LayoutRow();
    }


    void IDraw.Draw(SpriteBatch spriteBatch)
    {
        DrawUI(spriteBatch);
    }

    void DrawUI(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            if (!child.Visible) continue;
            child?.Draw(spriteBatch);
        }
    }
}