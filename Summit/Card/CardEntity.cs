using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Card;

public class CardEntity(CardData data) : Entity(data.CreateSprite(MainGame.Atlas))
{
    public CardData Data { get; init; } = data;

    public bool IsSelected => MainGame.MainHand.Selected.Contains(this);
    public void SetSelected(bool val)
    {
        if (val && !IsSelected) MainGame.MainHand.Selected.Add(this);
        else if (!val) MainGame.MainHand.Selected.Remove(this);
    }

    public override void OnClick(MouseState state)
    {
        base.OnClick(state);

        if (MoveTarget is not null || !(MainGame.MainHand.Entities.Contains(this)) || IsBeingDragged) return;

        if (IsSelected)
        {
            SetSelected(false);
            return;
        }

        SetSelected(true);
    }

    public override void OnDrag(MouseState state, Vector2 dragOffset)
    {
        base.OnDrag(state, dragOffset);

        if (MoveTarget is not null || !(MainGame.MainHand.Entities.Contains(this))) return;

        Position = state.Position.ToVector2() - dragOffset;
        MainGame.MainHand.UpdateIndex(this);
        SetSelected(false);
    }

    public override void OnRelease(MouseState state, bool wasBeingDragged)
    {
        base.OnRelease(state, wasBeingDragged);

        if (MoveTarget is not null || !(MainGame.MainHand.Entities.Contains(this))) return;

        if (wasBeingDragged) { 
            MainGame.MainHand.UpdateIndex(this);
        }
    }
}