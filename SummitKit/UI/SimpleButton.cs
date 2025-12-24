using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;

public class SimpleButton : Button
{
    private Texture2D _pixel;
    private readonly SpriteFont _font;
    private Shadow _shadow;
    private Vector2 _initDims;
    private Vector2 _dims;
    private Vector2 _scale;
    private bool _shadowEnabled;
    private Color _initBaseColour;
    private Color _initHoverColour;
    private bool _enabled = true;
    public Color? Colour { get; set; } = null;
    public Color BaseColour { get; set; } = Color.White;
    public Color HoverColour { get; set; } = Color.LightGray;
    public string Text { get; set; }
    public Vector2 TextPadding { get; set; } = new(20, 20);
    public int Radius { get; set; } = 8;
    public Action<SimpleButton, GameTime> OnUpdate { get; set; }
    public bool Enabled { get => _enabled; set {
            if (value == _enabled) return;

            _enabled = value;
            if (!_enabled)
            {
                _initBaseColour = BaseColour;
                _initHoverColour = HoverColour;
                BaseColour = Color.DarkGray;
                HoverColour = Color.DarkGray;

                if (Shadow.Enabled)
                {
                    _shadowEnabled = true;
                    Shadow.Enabled = false;
                }
            }
            else
            {
                BaseColour = _initBaseColour;
                HoverColour = _initHoverColour;

                if (_shadowEnabled)
                {
                    Shadow.Enabled = true;
                }
            }
        }
    }
    public override Shadow Shadow
    {
        get
        {
            _shadow ??= new();
            return _shadow;
        }
    }

    public Shadow TextShadow;
    public override float Width
    {
        get => _dims.X;
        protected set
        {
            _initDims.X = value;
            _dims.X = value * Scale.X;
            _pixel = UIContainer.CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)_dims.X, (int)_dims.Y, Radius, Color.White);
        }
    }
    public override float Height
    {
        get => _dims.Y;
        protected set
        {
            _initDims.Y = value;
            _dims.Y = value * Scale.Y;
                _pixel = UIContainer.CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)_dims.X, (int)_dims.Y, Radius, Color.White);
            }
    }
    public override Vector2 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _dims = _initDims * _scale;
        }
    }

    public SimpleButton(SpriteFont font, Action<Button> callback, bool onRelease = false) : base(null, callback, onRelease)
    {
        TextShadow = new()
        {
            Offset = new Vector2(3, 3)
        };
        Scale = Vector2.One;
        Colour = BaseColour;
        _font = font;
    }


    public void SetDimensions(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public void SetDimensions(Vector2 dims)
    {
        Width = dims.X;
        Height = dims.Y;
    }

    public override void OnHover(MouseState state)
    {
        Colour = HoverColour;
    }

    public override void OnHoverStop(MouseState state)
    {
        Colour = BaseColour;
    }

    public override void OnClick(MouseState input)
    {
        if (Enabled) { 
            base.OnClick(input);
        }
        if (Shadow is not null)
        {
            if (!Shadow.Enabled)
            {
                _shadowEnabled = false;
                return;
            }

            _shadowEnabled = true;
            Shadow.Enabled = false;

            Position += Shadow.Offset;
        }
    }

    public override void OnRelease(MouseState input, bool wasBeingDragged)
    {
        if (Enabled)
        {
            base.OnRelease(input, wasBeingDragged);
        }

        if (Shadow is not null && _shadowEnabled)
        {
            Shadow.Enabled = true;

            Position -= Shadow.Offset * 0.5F;
        }
    }

    public override void Update(GameTime time)
    {
        base.Update(time);

        OnUpdate?.Invoke(this, time);
    }

    public override void Draw(SpriteBatch batch)
    {
        Colour ??= BaseColour;

        if (Colour.Value != BaseColour && Colour.Value != HoverColour)
        {
            Colour = BaseColour;
        }

        // draw shadow
        Shadow?.Draw(batch, _pixel,
            new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height),
            null,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None);

        // fill a rectangle with the button color
        batch.Draw(
            _pixel,
            new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height),
            null,
            Colour.Value,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            0.1F
        );

        // draw text on top at the centre
        if (string.IsNullOrEmpty(Text)) return;

        var wrapped = Core.Console.WordWrap(Text, Width - 10, _font);
        float y = Position.Y + Height / 2;

        foreach (var line in wrapped) {
            float scale = 0F;

            var textSize = _font!.MeasureString(line);
            bool over = false;
            while (!over)
            {
                over = (textSize * scale).X > Width - TextPadding.X || (textSize * scale).Y > Height - TextPadding.Y;
                if (over) break;

                scale += .1F;
            }
            textSize *= scale;

            var textPosition = new Vector2(
                Position.X + Width / 2 - textSize.X / 2,
                y - textSize.Y / 2
            );

            TextShadow?.DrawString(
                batch,
                _font!,
                line,
                textPosition,
                0F,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0.15F
            );
            batch.DrawString(
                _font,
                line,
                textPosition,
                Color.White,
                0F,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0.15F
            );

            y += textSize.Y;
        }
    }
}