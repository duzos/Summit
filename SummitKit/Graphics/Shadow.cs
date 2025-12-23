using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Graphics;

public class Shadow {
    public Color Colour { get; set; } = new Color(0, 0, 0, 0.25F);
    public bool Enabled { get; set; } = true;
    public Vector2 Offset { get; set; } = new Vector2(0, 10);

    public void Draw(SpriteBatch batch, Sprite sprite, Vector2 position)
    {
        if (!Enabled) return;

        Enabled = false;

        var beforeColour = sprite.Colour;
        sprite.Colour = Colour;
        var beforeDepth = sprite.LayerDepth;
        sprite.LayerDepth = 0.0F;
        sprite.Draw(batch, position + Offset);
        sprite.Colour = beforeColour;
        sprite.LayerDepth = beforeDepth;

        Enabled = true;
    }
}