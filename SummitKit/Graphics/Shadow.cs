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

    public void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth = 0.01F)
    {
        if (!Enabled) return;
        Enabled = false;
        layerDepth -= 0.01F;
        Vector2 position = new Vector2(destinationRectangle.X, destinationRectangle.Y) + Offset;
        Rectangle newDestinationRectangle = new Rectangle((int)position.X, (int)position.Y, destinationRectangle.Width, destinationRectangle.Height);
        spriteBatch.Draw(texture, newDestinationRectangle, sourceRectangle, Colour, rotation, origin, effects, layerDepth);
        Enabled = true;
    }

    public void DrawString(SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth = 0.01F)
    {
        if (!Enabled) return;
        Enabled = false;
        spriteBatch.DrawString(spriteFont, text, position + Offset, Colour, rotation, origin, scale, effects, layerDepth - 0.01F);
        Enabled = true;
    }
}