using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Physics;

public class Button : Entity
{
    private Action<Button> _callback;
    private bool _onRelease;

    public Button(Sprite sprite, Action<Button> callback, bool onRelease = false) : base(sprite)
    {
        HasCollisions = false;
        HasGravity = false;
        Draggable = false;
        _callback = callback;
        _onRelease = onRelease;
    }

    public override void OnClick(MouseState input)
    {
        if (!_onRelease)
            _callback?.Invoke(this);
        base.OnClick(input);
    }

    public override void OnRelease(MouseState input, bool wasBeingDragged)
    {
        if (_onRelease)
            _callback?.Invoke(this);
        base.OnRelease(input, wasBeingDragged);
    }
}