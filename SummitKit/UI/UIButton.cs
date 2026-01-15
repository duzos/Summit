using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI;

public class UIButton : UIContainer
{
    private readonly Action<UIButton> _callback;
    private readonly bool _onRelease;
    private bool _shadowEnabled;
    private Color _initBaseColour;
    private Color _initHoverColour;
    private bool _enabled = true;
    public Color BaseColour { get; set; } = Color.White;
    public Color HoverColour { get; set; } = Color.LightGray;

    public bool Enabled
    {
        get => _enabled; set
        {
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

    public override Vector2 Position
    {
        get => Layout.Location.ToVector2();
        set
        {
            PreferredLayout = new Rectangle((int)value.X, (int)value.Y, PreferredLayout.Width, PreferredLayout.Height);
            Layout = PreferredLayout;


            // Parent?.RecalculateLayout();
            ((IUIElement)this).RecalculateLayout();
        }
    }

    public UIButton(Action<UIButton> callback, bool onRelease = true)
    {
        _callback = callback;
        _onRelease = onRelease;
        BackgroundColour = BaseColour;
        Clickable = true;
        Padding = 0;
        Spacing = 0;
    }

    public override void OnHover(MouseState state)
    {
        BackgroundColour = HoverColour;
    }

    public override void OnHoverStop(MouseState state)
    {
        BackgroundColour = BaseColour;
    }

    public override void OnClick(MouseState input)
    {
        if (Enabled)
        {
            if (!_onRelease)
                _callback?.Invoke(this);
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
            if (_onRelease)
                _callback?.Invoke(this);
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
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (BackgroundColour != BaseColour && BackgroundColour != HoverColour)
        {
            BackgroundColour = BaseColour;
        }

        base.Draw(spriteBatch);
    }
}