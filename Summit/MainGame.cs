using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Summit.Card;
using Summit.Command;
using Summit.Scenes;
using Summit.State;
using SummitKit;
using SummitKit.Audio;
using SummitKit.Command;
using SummitKit.Graphics;
using SummitKit.Input;
using SummitKit.IO;
using SummitKit.Physics;
using SummitKit.UI;
using SummitKit.UI.Scene;
using System;
using System.Collections;
using System.Linq;
using System.Net.WebSockets;
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

        public static CardSounds CardSounds { get; } = new CardSounds();

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
            _background = Content.Load<Effect>("assets/Background");
            SummitSceneExtensions.Initialize();

            LoadMusic();
            CardSounds.LoadContent(Content);
        }

        private void LoadMusic()
        {
            LoadMusic("cool_vibes");
            LoadMusic("airport_lounge");
        }

        private Song LoadMusic(string name)
        {
            var song = Content.Load<Song>($"assets/audio/music/{name}");
            MusicTracker music = Audio.Music;
            music.AddSong(song);

            return song;
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

            State.Update(gameTime);
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

            State.MainHand.Draw(SpriteBatch);
            State.PlayedHand.Draw(SpriteBatch);

            SpriteBatch.End();
        }
    }
}