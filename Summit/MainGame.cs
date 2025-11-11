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

namespace Summit
{
    public class MainGame : Core
    {
        public static TextureAtlas Atlas { get; private set; }
        public static Deck MainDeck { get; private set; }
        public static Hand MainHand { get; private set; }

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
            MainHand = new()
            {
                MaxSize = 5
            };

            var button = new Button(Atlas.CreateSprite("ace-hearts"), but =>
            {
                MainHand.Clear();
                MainHand.DespawnCards();
                MainDeck.AddAll(MainHand.Cards);
                MainDeck.Deal(MainHand);
                MainHand.SpawnCards();
            });
            // bottom right corner
            button.Scale *= 2F;
            button.Position = new Vector2(1280 - button.Width - 10, 720 - button.Height - 10);

            Entities.AddEntity(button);
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

            SpriteBatch.End();
        }
    }
}