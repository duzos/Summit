using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summit.Card;
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
        public static Deck MainDeck { get; private set; }
        public static Hand MainHand { get; private set; }

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
            MainDeck = new();
            MainDeck.Shuffle();
            MainHand = new();

            var button = new Button(Atlas.CreateSprite("ace-hearts"), but =>
            {
                MainDeck.AddAll(MainHand.Selected);
                MainHand.DiscardSelected();
                MainDeck.Deal(MainHand);
                MainHand.SpawnCards();
            });
            // bottom right corner
            button.Scale *= 2F;
            button.Position = new Vector2(1280 - button.Width - 10, 720 - button.Height - 10);

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


            // Begin the sprite batch to prepare for rendering.
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            base.Draw(gameTime);
            string score = MainHand.TotalValue().ToString();
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
            SpriteBatch.End();
        }
    }
}