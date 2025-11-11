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

    public override void OnClick(MouseState state)
    {
        base.OnClick(state);

        if (MoveTarget is not null || !(MainGame.MainHand.Entities.Contains(this))) return;

        if (MainGame.MainHand.Selected.Contains(this))
        {
            MainGame.MainHand.Selected.Remove(this);
            return;
        }

        MainGame.MainHand.Selected.Add(this);
    }
}