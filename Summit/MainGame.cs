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
            _font = Content.Load<SpriteFont>("assets/balatro");
            ConsoleFont = _font;

            base.LoadContent();

            Atlas = TextureAtlas.FromFile(Content, "assets/atlas-definition.xml");
            State = new();

            var button = new SimpleButton(ConsoleFont, but =>
            {
                State.PlaySelected();
                State.Deal();
            });
            button.SetDimensions(200, 100);
            button.Shadow.Enabled = true;
            button.Scale = Vector2.One;
            // next to the other button
            button.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - button.Width - 55F, GraphicsDevice.PresentationParameters.BackBufferHeight - 15 - button.Height);
            float lastX = button.Position.X + button.Width;
            button.Text = "Play";
            button.BaseColour = Color.Blue;
            button.HoverColour = Color.DarkBlue;
            button.OnUpdate = (b, time) => {
                b.Text = "Play (" + State.RemainingHands + ")";
                b.Enabled = State.RemainingHands > 0;
            };
            Entities.AddEntity(button);

            button = new SimpleButton(ConsoleFont, but =>
            {
                State.MainHand.SortCards(Hand.SortByValue);
                State.LastSort = Hand.SortByValue;
            });
            button.SetDimensions(100, 50);
            button.Scale = Vector2.One;
            button.Position = new(lastX + 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 10 - button.Height);
            button.Text = "Rank";
            button.BaseColour = Color.Orange;
            button.HoverColour = Color.DarkOrange;
            Entities.AddEntity(button);

            button = new SimpleButton(ConsoleFont, but =>
            {
                State.MainHand.SortCards(Hand.SortBySuit);
                State.LastSort = Hand.SortBySuit;
            });
            button.SetDimensions(100, 50);
            button.Scale = Vector2.One;
            button.Position = new(lastX + 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 15 - button.Height - button.Height);
            button.Text = "Suit";
            lastX += button.Width + 5;
            button.BaseColour = Color.Orange;
            button.HoverColour = Color.DarkOrange;
            Entities.AddEntity(button);

            button = new SimpleButton(ConsoleFont, but =>
            {
                State.DiscardSelected();
            });
            button.SetDimensions(200, 100);
            button.Position = new(lastX + 5, GraphicsDevice.PresentationParameters.BackBufferHeight - 15 - button.Height);
            button.Shadow.Enabled = true;
            button.Text = "Discard";
            button.BaseColour = Color.Red;
            button.HoverColour = Color.DarkRed;
            button.OnUpdate = (b, time) => {
                b.Text = "Discard (" + State.RemainingDiscards + ")";

                b.Enabled = State.RemainingDiscards > 0;
            };

            Entities.AddEntity(button);

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
                new(10, 200),           // position
                Color.White,             // color
                0.0F,
                Vector2.Zero,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            SpriteBatch.DrawString(
                _font,                   // font
                State.TargetScore.ToString(),     // text
                new(10, 50),           // position
                Color.White,             // color
                0.0F,
                Vector2.Zero,
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
            /*SpriteBatch.DrawString(
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
            );*/
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