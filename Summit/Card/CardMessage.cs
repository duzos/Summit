using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummitKit;
using SummitKit.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Summit.Card;

/// <summary>
/// Displays a rotated cube with text on it that fades out over time. 
/// </summary>
public class CardMessage : Entity
{
    public CardMessage() : base(null)
    {
        Scale = Vector2.One;
        HasCollisions = false;
        HasGravity = false;
        Draggable = false;
        Clickable = false;
    }

    public string Message { get; set; } = string.Empty;
    public Color Colour { get; set; } = Color.CornflowerBlue;
    public Color TextColour { get; set; } = Color.White;
    public Color? TextOutlineColour { get; set; } = Color.Black;
    public float TextOutlineThickness { get; set; } = 2f;
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromSeconds(2);
    private TimeSpan _elapsed = TimeSpan.Zero;
    public float Alpha => 1f - (float)(_elapsed.TotalMilliseconds / Lifetime.TotalMilliseconds);
    private Texture2D? _cubeTexture;

    public override void Update(GameTime time)
    {
        base.Update(time);
        _elapsed += time.ElapsedGameTime;
        if (_elapsed >= Lifetime)
        {
            Remove();
        }
    }

    public SpriteFont? Font { get; set; }
    private float _width = 50f;
    private float _height = 50f;

    public override float Width
    {
        get => _width;
        protected set => _width = value;
    }

    public override float Height
    {
        get => _height;
        protected set => _height = value;
    }

    public void SetSize(float width, float height)
    {
        _width = width;
        _height = height;
        InvalidateAABB();
    }

    public override void Draw(SpriteBatch batch)
    {
        base.Draw(batch);

        if (_cubeTexture is null)
        {
            _cubeTexture = new Texture2D(batch.GraphicsDevice, 1, 1);
            _cubeTexture.SetData(new[] { Color.White });
        }

        Vector2 center = Centre;
        float alpha = Alpha;
        Color squareColor = Colour * alpha;

        batch.Draw(
            _cubeTexture,
            center,
            null,
            squareColor,
            Rotation,
            new Vector2(0.5f, 0.5f), // origin at center of 1x1 texture
            new Vector2(Width, Height),
            SpriteEffects.None,
            0f
        );

        if (!string.IsNullOrEmpty(Message) && Font is not null)
        {
            var wrapped = Core.Console.WordWrap(Message, Width - 10, Font);
            Vector2 textPos = center;

            foreach (var line in wrapped)
            {
                float scale = 0F;

                var textSize = Font!.MeasureString(line);
                bool over = false;
                while (!over)
                {
                    over = (textSize * scale).X > Width || (textSize * scale).Y > Height;
                    if (over) break;

                    scale += .1F;
                }

                scale -= .2F;
                textSize *= scale;
                textPos -= (textSize / 2);

                Color textColor = TextColour * alpha;

                if (TextOutlineColour.HasValue)
                {
                    Color outlineColor = TextOutlineColour.Value * (alpha - 0.1F);

                    for (float ox = -TextOutlineThickness; ox <= TextOutlineThickness; ox += TextOutlineThickness)
                    {
                        for (float oy = -TextOutlineThickness; oy <= TextOutlineThickness; oy += TextOutlineThickness)
                        {
                            if (ox != 0 || oy != 0)
                                batch.DrawString(Font, Message, textPos + new Vector2(ox, oy), outlineColor, 0, new Vector2(0.5f, 0.5f), 1, SpriteEffects.None, 0.1f);
                        }
                    }
                }

                batch.DrawString(Font, Message, textPos, textColor, 0, new Vector2(0.5f, 0.5f), 1, SpriteEffects.None, 0.2F);

                textPos += textSize / 2;
                textPos += new Vector2(0, textSize.Y);
            }
        }
    }
}