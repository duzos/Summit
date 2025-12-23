using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SummitKit;
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
    public Hand ParentHand { get; set; }

    public bool IsSelected => ParentHand.Selected.Contains(this);
    public void SetSelected(bool val)
    {
        if (ParentHand is null || !Draggable || !ParentHand.Draggable) return;

        if (val && !IsSelected) ParentHand.Selected.Add(this);
        else if (!val) ParentHand.Selected.Remove(this);

        if (Shadow is not null)
        {
            Shadow.Enabled = ParentHand.Selected.Contains(this);
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
        bool val = Backwards;
        ScaleTo(originalScale * new Vector2(0, 1), dur, delay, (target) =>
        {
            Backwards = !val;
            ScaleTo(originalScale, backDur, TimeSpan.Zero);
        }, replaceExisting: true);
    }

    public override void Update(GameTime time)
    {
        base.Update(time);

        // circular wobble rotation with variance based off index
        int index = (Core.Entities.Entities.OfType<CardEntity>().ToList().IndexOf(this) + 1);
        float wobble = 0.02f * MathF.Sin((float)(time.TotalGameTime.TotalSeconds * 2 + index));

        // tilt towards velocity
        float velocityTilt = (Velocity.X / 100f) * 0.05f;

        // make the final rotation the sum of wobble and velocity tilt, smoothing towards it
        float desiredRotation = wobble + velocityTilt;
        Rotation = MathHelper.Lerp(Rotation, desiredRotation, 0.1f);
    }
    public override void OnClick(MouseState state)
    {
        base.OnClick(state);

        if (MoveTarget is not null || ParentHand is null || !(ParentHand.Entities.Contains(this)) || IsBeingDragged) return;

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

        ParentHand.UpdateIndex(this);
        if (MoveTarget is not null || !(ParentHand.Entities.Contains(this))) return;
        
        SetSelected(false);
    }

    public override void OnRelease(MouseState state, bool wasBeingDragged)
    {
        base.OnRelease(state, wasBeingDragged);

        if (!(ParentHand.Entities.Contains(this))) return;

        if (wasBeingDragged) {
            Velocity = Vector2.Zero;
            MoveTarget = null;
            ParentHand.UpdateIndex(this);
            ParentHand.UpdatePositions();
        }
    }
}