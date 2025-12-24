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

public class UIContainer : Entity, IUIElement
{
    private Texture2D _pixel;
    private Shadow _shadow;
    private float _layerDepth;
    protected List<IUIElement> _children = [];

    public UIContainer() : base(null!)
    {
        Scale = Vector2.One;

        HasCollisions = false;
        HasGravity = false;
        Draggable = false;
        Clickable = false;
    }

    public bool Visible { get; set; } = true;
    public Rectangle Layout { get; set; }
    public IUIElement? Parent { get; set; }

    public Rectangle PreferredLayout { get; set; }

    public UIAlign HorizontalAlign { get; set; } = UIAlign.Start;
    public UIAlign VerticalAlign { get; set; } = UIAlign.Start;
    public bool ForceNewRow { get; set; } = false;

    public int Padding { get; set; } = 5;
    public int Spacing { get; set; } = 5;
    public int Radius { get; set; } = 16;
    public override float LayerDepth { get => _layerDepth; set => _layerDepth = value; }
    public Color BackgroundColour { get; set; } = Color.White;

    public List<IUIElement> Children => _children;
    public override Shadow Shadow
    {
        get
        {
            _shadow ??= new();
            return _shadow;
        }
    }

    public override Rectangle AABB
    {
        get => Layout;
    }

    public override float Width
    {
        get => Layout.Width;
        protected set
        {
            var preferredLayout = PreferredLayout;
            preferredLayout.Width = (int)value;
            PreferredLayout = preferredLayout;
            if (Parent is null)
            {
                Layout = preferredLayout;
            }
            Parent?.RecalculateLayout();
            ((IUIElement)this).RecalculateLayout();
            _pixel = CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)Layout.Width, (int)Layout.Height, Radius, Color.White);
        }
    }
    public override float Height
    {
        get => Layout.Height;
        protected set
        {
            var preferredLayout = PreferredLayout;
            preferredLayout.Height = (int)value;
            PreferredLayout = preferredLayout;

            if (Parent is null)
            {
                Layout = preferredLayout;
            }

            Parent?.RecalculateLayout();
            ((IUIElement)this).RecalculateLayout();
            _pixel = CreateRoundedRectangle(Core.Graphics.GraphicsDevice, (int)Layout.Width, (int)Layout.Height, Radius, Color.White);
        }
    }

    public override Vector2 Position
    {
        get => Layout.Location.ToVector2();
        set
        {
            PreferredLayout = new Rectangle((int) value.X, (int) value.Y, PreferredLayout.Width, PreferredLayout.Height);
            if (Parent is null)
            {
                Layout = PreferredLayout;
            }

            Parent?.RecalculateLayout();
            ((IUIElement)this).RecalculateLayout();
        }
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

    public override void Update(GameTime deltaTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // draw shadow
        Shadow?.Draw(spriteBatch, _pixel,
            Layout,
            null,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth);

        // fill a rectangle with the container color
        spriteBatch.Draw(
            _pixel,
            Layout,
            null,
            BackgroundColour,   
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth
        );

        if (IUIElement.Debug)
        {
            DrawOutline(spriteBatch, IUIElement.Thickness);
        }

        base.Draw(spriteBatch);

        ((IUIElement)this).DrawUI(spriteBatch);
    }

    public void DrawOutline(SpriteBatch sb, int thickness)
    {
        Rectangle r = Layout;
        Color outline = Color.Green;

        // Top
        sb.Draw(
            _pixel,
            new Rectangle(r.X, r.Y, r.Width, thickness),
            null,
            outline,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth + 0.01F
        );

        // Bottom
        sb.Draw(
            _pixel,
            new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness),
            null,
            outline,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth + 0.01F
        );

        // Left
        sb.Draw(
            _pixel,
            new Rectangle(r.X, r.Y, thickness, r.Height),
            null,
            outline,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth + 0.01F
        );

        // Right
        sb.Draw(
            _pixel,
            new Rectangle(r.Right - thickness, r.Y, thickness, r.Height),
            null,
            outline,
            Rotation,
            Vector2.Zero,
            SpriteEffects.None,
            LayerDepth + 0.01F
        );
    }


    public static Texture2D CreateRoundedRectangle(GraphicsDevice graphicsDevice, int width, int height, int radius, Color color)
    {
        if (width <= 0 || height <= 0 || radius <= 0) return new Texture2D(graphicsDevice, 1, 1);

        Texture2D texture = new(graphicsDevice, width, height);
        Color[] data = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside =
                    x >= radius && x < width - radius || // inside horizontal middle
                    y >= radius && y < height - radius;  // inside vertical middle

                // check corners
                int dx = 0, dy = 0;
                if (x < radius) dx = radius - x;
                else if (x >= width - radius) dx = x - (width - radius - 1);

                if (y < radius) dy = radius - y;
                else if (y >= height - radius) dy = y - (height - radius - 1);

                if (dx != 0 || dy != 0)
                {
                    inside |= dx * dx + dy * dy <= radius * radius;
                }

                data[y * width + x] = inside ? color : Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
    }
}