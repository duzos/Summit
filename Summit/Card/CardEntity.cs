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

        if (Shadow is not null)
        {
            Shadow.Enabled = MainGame.MainHand.Selected.Contains(this);
        }
    }

    public bool Backwards
    {
        get => Data.Backwards;
        set
        {
            Data.Backwards = value;

            var preSprite = Sprite;
            Sprite = Data.CreateSprite(MainGame.Atlas);

            if (preSprite is not null && Sprite is not null)
            {
                preSprite.CopyTo(Sprite);
            }
        }
    }

    public void Flip(TimeSpan dur, TimeSpan delay, TimeSpan backDur)
    {
        if (ScaleTarget is not null) return;

        // Flip the card by scaling to zero width, changing the sprite, then scaling back to full width.
        Vector2 originalScale = Scale;
        ScaleTo(originalScale * new Vector2(0, 1), dur, delay, (target) =>
        {
            Backwards = !Backwards;
            ScaleTo(originalScale, backDur, TimeSpan.Zero);
        }, replaceExisting: true);
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

        MainGame.MainHand.UpdateIndex(this);
        if (MoveTarget is not null || !(MainGame.MainHand.Entities.Contains(this))) return;
        
        SetSelected(false);
    }

    public override void OnRelease(MouseState state, bool wasBeingDragged)
    {
        base.OnRelease(state, wasBeingDragged);

        if (!(MainGame.MainHand.Entities.Contains(this))) return;

        if (wasBeingDragged) {
            Velocity = Vector2.Zero;
            MoveTarget = null;
            MainGame.MainHand.UpdateIndex(this);
            MainGame.MainHand.UpdatePositions();
        }
    }
}