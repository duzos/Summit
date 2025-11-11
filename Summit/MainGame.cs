using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit;

namespace Summit
{
    public class MainGame : Core
    {
        private Texture2D _test;

        public MainGame() : base("Summit", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _test = Content.Load<Texture2D>("assets/ace");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AliceBlue);

            SpriteBatch.Begin();
            MouseState mouse = Mouse.GetState();
            SpriteBatch.Draw(_test, new Vector2(mouse.X - (_test.Width / 2), mouse.Y - (_test.Height / 2)), Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}