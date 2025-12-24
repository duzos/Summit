using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Input;

public interface IClickable
{
    bool Clickable { get; set; }

    void OnClick(MouseState state);
    void OnRelease(MouseState state, bool wasBeingDragged);
    void OnHover(MouseState state);
    void OnHoverStop(MouseState state);
    void OnDrag(MouseState state, Vector2 dragOffset);
}
