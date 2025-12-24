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
using SummitKit.UI;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Resources;
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

            CreatePlaySegment();

            _background = Content.Load<Effect>("assets/Background");
        }

        private static UIContainer CreatePlaySegment()
        {
            UIContainer container = new()
            {
                BackgroundColour = Color.Transparent
            };
            container.Shadow.Enabled = false;
            container.SetDimensions(520, 100);
            container.Position = new((GraphicsDevice.PresentationParameters.BackBufferWidth / 2) - (container.Width / 2), GraphicsDevice.PresentationParameters.BackBufferHeight - 15 - container.Height);
            container.Add();

            var text = new UIText(ConsoleFont)
            {
                VerticalAlign = UIAlign.Center,
                HorizontalAlign = UIAlign.Center,
                TextHorizontalAlign = UIAlign.Center,
                OnUpdate = (t) =>
                {
                    t.Text = "Play (" + State.RemainingHands + ")";
                }
            };

            var button = new UIButton(but =>
            {
                State.PlaySelected();
                State.Deal();
            });
            button.SetDimensions(200, 100);
            text.SetDimensions(button.PreferredLayout.Size.ToVector2() - new Vector2(button.Padding));
            button.Shadow.Enabled = true;
            button.BaseColour = Color.Blue;
            button.HoverColour = Color.DarkBlue;
            button.OnUpdate = (b, time) => {
                b.Enabled = State.RemainingHands > 0;
            };
            ((IUIElement)button).AddChild(text);
            text.Add();
            button.Add();
            ((IUIElement) container).AddChild(button);

            var sortContainer = new UIContainer()
            {
                BackgroundColour = Color.Transparent,
                Padding = 0,
                Spacing = 10
            };
            sortContainer.SetDimensions(100, 100);
            sortContainer.Shadow.Enabled = false;

            var sortText = new UIText(ConsoleFont, "Rank")
            {
                VerticalAlign = UIAlign.Center,
                HorizontalAlign = UIAlign.Center,
                TextHorizontalAlign = UIAlign.Center
            };
            button = new UIButton(but =>
            {
                State.MainHand.SortCards(Hand.SortByValue);
                State.LastSort = Hand.SortByValue;
            })
            {
                Radius = 8
            };
            button.SetDimensions(100, 45);
            sortText.SetDimensions(button.PreferredLayout.Size.ToVector2() / new Vector2(2, 1));
            button.BaseColour = Color.Orange;
            button.HoverColour = Color.DarkOrange;
            ((IUIElement) button).AddChild(sortText);
            ((IUIElement)sortContainer).AddChild(button);
            sortText.Add();
            button.Add();

            sortText = new UIText(ConsoleFont, "Suit")
            {
                VerticalAlign = UIAlign.Center,
                HorizontalAlign = UIAlign.Center,
                TextHorizontalAlign = UIAlign.Center
            };
            button = new UIButton(but =>
            {
                State.MainHand.SortCards(Hand.SortBySuit);
                State.LastSort = Hand.SortBySuit;
            })
            {
                Radius = 8
            };
            button.SetDimensions(100, 45);
            sortText.SetDimensions(button.PreferredLayout.Size.ToVector2() / new Vector2(2, 1));
            button.BaseColour = Color.Orange;
            button.HoverColour = Color.DarkOrange;
            ((IUIElement)button).AddChild(sortText);
            ((IUIElement)sortContainer).AddChild(button);
            sortText.Add();
            button.Add();

            sortContainer.Add();
            ((IUIElement)container).AddChild(sortContainer);

            text = new UIText(ConsoleFont)
            {
                VerticalAlign = UIAlign.Center,
                HorizontalAlign = UIAlign.Center,
                TextHorizontalAlign = UIAlign.Center,
                OnUpdate = (t) =>
                    {
                        t.Text = "Discard (" + State.RemainingDiscards + ")";
                    }
            };

            button = new UIButton(but =>
            {
                State.DiscardSelected();
            });
            button.SetDimensions(200, 100);
            text.SetDimensions(button.PreferredLayout.Size.ToVector2() - new Vector2(button.Padding));
            button.Shadow.Enabled = true;
            button.BaseColour = Color.Red;
            button.HoverColour = Color.DarkRed;
            button.OnUpdate = (b, time) => {
                b.Enabled = State.RemainingDiscards > 0;
            };
            ((IUIElement)button).AddChild(text);
            text.Add();
            button.Add();
            ((IUIElement)container).AddChild(button);

            ((IUIElement)container).RecalculateLayout();

            return container;
        }

        public override void ToggleFullScreen()
        {
            base.ToggleFullScreen();

            // todo reload all entities
            State.OnLoad();
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
            _background.Parameters["iTime"]?.SetValue(totalTime);
            _background.Parameters["SpiralFactor"]?.SetValue(1.5F);
            _background.Parameters["RotationSpeed"]?.SetValue(0.15F);
            _background.Parameters["RingSpacing"]?.SetValue(0.25F);

            // Set colors (RGBA)
            //_background.Parameters["colour3"].SetValue(new Vector4(0.18f, 0.37f, 0.29f, 1f)); // dark felt
            //_background.Parameters["colour2"].SetValue(new Vector4(0.23f, 0.46f, 0.36f, 1f));  // lighter green
            //_background.Parameters["colour1"].SetValue(new Vector4(0.27f, 0.52f, 0.42f, 1f));  // subtle highlight

            // Pass screen size
            _background.Parameters["iResolution"].SetValue(new Vector2(
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
                "Current\n" + score,     // text
                new(10, 210),           // position
                Color.White,             // color
                0.0F,
                Vector2.Zero,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            SpriteBatch.DrawString(
                _font,                   // font
                "Target\n" + State.TargetScore.ToString(),     // text
                new(10, 50),           // position
                Color.White,             // color
                0.0F,
                Vector2.Zero,
                5.0F,
                SpriteEffects.None,
                0.0f
            );

            if (State.PlayedScore.HasValue)
            {
                SpriteBatch.DrawString(
                    _font,                   // font
                    State.PlayedScore.Value.ToString(),     // text
                    new(GraphicsDevice.Viewport.Width / 2, 100),           // position
                    Color.White,             // color
                    0.0F,
                    _font.MeasureString(State.PlayedScore.Value.ToString()) * 0.5F,
                    5.0F,
                    SpriteEffects.None,
                    0.0f
                );
            }

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
                new(GraphicsDevice.PresentationParameters.BackBufferWidth - 60, GraphicsDevice.PresentationParameters.BackBufferHeight - 180),           // position
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