using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;

public class UIText : UIContainer
{
    private readonly SpriteFont _font;
    public string Text { get; set; }
    public UIAlign TextHorizontalAlign { get; set; } = UIAlign.Center;
    public UIAlign TextVerticalAlign { get; set; } = UIAlign.Center;
    public Action<UIText>? OnUpdate { get; set; }

    public UIText(SpriteFont font, string text = "", Action<UIText>? onUpdate = null) : base()
    {
        _font = font;
        Text = text;
        Shadow.Offset = new(2, 3);
        OnUpdate = onUpdate;
    }

    public override void Update(GameTime deltaTime)
    {
        base.Update(deltaTime);

        OnUpdate?.Invoke(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        ((IUIElement)this).DrawUI(spriteBatch);

        if (IUIElement.Debug)
        {
            DrawOutline(spriteBatch, IUIElement.Thickness);
        }

        // draw text on top at the centre
        if (string.IsNullOrEmpty(Text)) return;

        var wrapped = Core.Console.WordWrap(Text, Width - 10, _font);
        float y = Position.Y + Height;

        foreach (var line in wrapped)
        {
            float scale = 0F;

            var textSize = _font!.MeasureString(line);
            bool over = false;
            while (!over)
            {
                over = (textSize * scale).X > Width || (textSize * scale).Y > Height;
                if (over) break;

                scale += .1F;
            }

            scale -= .2F;
            textSize *= scale;

            var textPosition = Position;
            
            switch (TextHorizontalAlign)
            {
                case UIAlign.Center:
                    textPosition.X += (Width / 2) - (textSize.X / 2);
                    break;
                case UIAlign.End:
                    textPosition.X += Width - textSize.X;
                    break;
            }

            switch (TextVerticalAlign) {                 
                case UIAlign.Center:
                    textPosition.Y += (Height / 2) - (textSize.Y / 2);
                    break;
                case UIAlign.End:
                    textPosition.Y += Height;
                    break;
            }

            Shadow?.DrawString(
                spriteBatch,
                _font!,
                line,
                textPosition,
                0F,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                LayerDepth
            );
            spriteBatch.DrawString(
                _font,
                line,
                textPosition,
                BackgroundColour,
                0F,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                LayerDepth
            );

            y += textSize.Y;
        }
    }
}