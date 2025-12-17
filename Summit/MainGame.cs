using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summit.Card;
using Summit.State;
using SummitKit;
using SummitKit.Graphics;
using SummitKit.Input;
using SummitKit.Physics;
using System;
using System.Collections;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Summit
{
    public class MainGame : Core
    {
        public static TextureAtlas Atlas { get; private set; }
        public static GameState State { get; private set; }

        private SpriteFont _font;
        public MainGame() : base("Summit", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Atlas = TextureAtlas.FromFile(Content, "assets/atlas-definition.xml");
            State = new();

            var button = new Button(Atlas.CreateSprite("red-back"), but =>
            {
                State.MainDeck.AddAll(State.MainHand.Selected);
                State.MainHand.DiscardSelected();
                State.Deal();
                State.MainHand.SpawnCards();
            });
            button.Shadow.Enabled = true;
            // bottom right corner
            button.Scale *= 2F;
            button.Position = new Vector2(1280 - button.Width - 10, 720 - button.Height - 10);

            Entities.AddEntity(button);

            button = new(Atlas.CreateSprite("blue-back"), but =>
            {
                State.PlaySelected();
                State.Deal();
            });
            button.Shadow.Enabled = true;
            button.Scale *= 2F;
            // next to the other button
            button.Position = new Vector2(1280 - button.Width - button.Width - 20, 720 - button.Height - 10);
            Entities.AddEntity(button);

            button = new Button(Atlas.CreateSprite("rank-btn"), but =>
            {
                State.MainHand.SortCards(Hand.SortByValue);
            });
            button.Scale *= 2F;
            button.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - button.Width - 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 10 - button.Height);
            Entities.AddEntity(button);

            button = new Button(Atlas.CreateSprite("suit-btn"), but =>
            {
                State.MainHand.SortCards(Hand.SortBySuit);
            });
            button.Scale *= 2F;
            button.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) + 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 10 - button.Height);

            Entities.AddEntity(button);

            _font = Content.Load<SpriteFont>("assets/primary");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            base.Draw(gameTime);

            string score = State.MainHand.TotalValue().ToString();
            SpriteBatch.DrawString(
                _font,                   // font
                score,     // text
                new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2), GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - 100),           // position
                Color.Black,             // color
                0.0F,
                _font.MeasureString(score) * 0.5F,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            SpriteBatch.DrawString(
                _font,
                Entities.DraggedEntity is not CardEntity e ? "" : State.MainHand.Cards.ToList().IndexOf(e.Data).ToString(),
                new(10, 10),
                Color.Black,
                0.0F,
                Vector2.Zero,
                1.0F,
                SpriteEffects.None,
                0.0f
                );
            SpriteBatch.End();
        }
    }
}