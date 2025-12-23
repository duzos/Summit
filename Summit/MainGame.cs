using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Summit.Card;
using Summit.Command;
using Summit.State;
using SummitKit;
using SummitKit.Command;
using SummitKit.Graphics;
using SummitKit.Input;
using SummitKit.IO;
using SummitKit.Physics;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Summit
{
    public class MainGame : Core
    {
        public static string Name => "Summit";

        public static TextureAtlas Atlas { get; private set; }
        public static GameState State { get; private set; }

        private SpriteFont _font;
        private Effect _background;
        public MainGame() : base("Summit", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();

            Console.Commands.RegisterNamespace("Summit.Command", assembly: typeof(SetScoreCommand).Assembly);

            try
            {
                ((ISerializable<GameState>)State).Load();
            }
            catch (Exception e)
            {
                Console.Context.Error("Failed to load game state: " + e.Message);
                State.NextRound();
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Atlas = TextureAtlas.FromFile(Content, "assets/atlas-definition.xml");
            State = new();

            var button = new Button(Atlas.CreateSprite("red-back"), but =>
            {
                State.DiscardSelected();
            });
            button.Shadow.Enabled = true;
            // bottom right corner
            button.Scale *= 2F;
            button.Sprite.LayerDepth = .1F;
            button.Position = new (GraphicsDevice.PresentationParameters.BackBufferWidth - button.Width - 10, GraphicsDevice.PresentationParameters.BackBufferHeight - button.Height - 10);

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
            button.Sprite.LayerDepth = .1F;
            Entities.AddEntity(button);

            button = new Button(Atlas.CreateSprite("rank-btn"), but =>
            {
                State.MainHand.SortCards(Hand.SortByValue);
                State.LastSort = Hand.SortByValue;
            });
            button.Scale *= 2F;
            button.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - button.Width - 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 10 - button.Height);
            button.Sprite.LayerDepth = .1F;
            Entities.AddEntity(button);

            button = new Button(Atlas.CreateSprite("suit-btn"), but =>
            {
                State.MainHand.SortCards(Hand.SortBySuit);
                State.LastSort = Hand.SortBySuit;
            });
            button.Scale *= 2F;
            button.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) + 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 10 - button.Height);
            button.Sprite.LayerDepth = .1F;

            Entities.AddEntity(button);

            _font = Content.Load<SpriteFont>("assets/balatro");
            _background = Content.Load<Effect>("assets/Background");
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

            float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
            // Set animation parameters
            _background.Parameters["time"]?.SetValue(totalTime);
            _background.Parameters["spin_time"]?.SetValue(totalTime * 0.1f); // can tweak speed
            _background.Parameters["spin_amount"].SetValue(0.25F);           // full swirl
            _background.Parameters["contrast"].SetValue(1);             // default contrast

            // Set colors (RGBA)
            _background.Parameters["colour3"].SetValue(new Vector4(0.18f, 0.37f, 0.29f, 1f)); // dark felt
            _background.Parameters["colour2"].SetValue(new Vector4(0.23f, 0.46f, 0.36f, 1f));  // lighter green
            _background.Parameters["colour1"].SetValue(new Vector4(0.27f, 0.52f, 0.42f, 1f));  // subtle highlight

            // Pass screen size
            _background.Parameters["screenSize"].SetValue(new Vector2(
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
            ));
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, _background);
            SpriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), GraphicsDevice.Viewport.Bounds, Color.White);
            SpriteBatch.End();

            SpriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);
                
            string score = State.Score.ToString();
            SpriteBatch.DrawString(
                _font,                   // font
                score,     // text
                new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2), GraphicsDevice.PresentationParameters.BackBufferHeight - 100),           // position
                Color.White,             // color
                0.0F,
                _font.MeasureString(score) * 0.5F,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            SpriteBatch.DrawString(
                _font,                   // font
                State.TargetScore.ToString(),     // text
                new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2), 50),           // position
                Color.White,             // color
                0.0F,
                _font.MeasureString(State.TargetScore.ToString()) * 0.5F,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            SpriteBatch.DrawString(
                _font,
                Entities.DraggedEntity is not CardEntity e ? "" : State.MainHand.Cards.ToList().IndexOf(e.Data).ToString(),
                new(10, 10),
                Color.White,
                0.0F,
                Vector2.Zero,
                1.0F,
                SpriteEffects.None,
                0.0f
                );

            // draw remaining hands & discards on top of respective buttons
            string txt = State.RemainingHands.ToString();
            SpriteBatch.DrawString(
                _font,                   // font
                txt,     // text
                new(GraphicsDevice.PresentationParameters.BackBufferWidth - 130, GraphicsDevice.PresentationParameters.BackBufferHeight - 58),           // position
                Color.White,             // color
                0.0F,
                _font.MeasureString(txt) * 0.5F,
                2.5F,
                SpriteEffects.None,
                0.11f
            );

            txt = State.RemainingDiscards.ToString();
            SpriteBatch.DrawString(
                _font,
                txt,
                new(GraphicsDevice.PresentationParameters.BackBufferWidth - 40, GraphicsDevice.PresentationParameters.BackBufferHeight - 58),
                Color.White,
                0.0F,
                _font.MeasureString(txt) * 0.5F,
                2.5F,
                SpriteEffects.None,
                0.11f
            );
            // remaining cards in deck count
            txt = (State.MainDeck.Count.ToString()) + "/" + (State.MainDeck.Count + State.DiscardDeck.Count + State.MainHand.Cards.Count).ToString();
            SpriteBatch.DrawString(
                _font,                   // font
                txt,     // text
                new(GraphicsDevice.PresentationParameters.BackBufferWidth - 50, GraphicsDevice.PresentationParameters.BackBufferHeight - 150),           // position
                Color.White,             // color
                0.0F,
                _font.MeasureString(txt) * 0.5F,
                1F,
                SpriteEffects.None,
                0.11f
            );
            base.Draw(gameTime);

            SpriteBatch.End();
        }
    }
}