using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SummitKit.Audio;
using SummitKit.Command;
using SummitKit.Input;
using SummitKit.Physics;
using SummitKit.UI;
using SummitKit.Util;
using System;

namespace SummitKit;

public class Core : Game
{
    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }


    /// <summary>
    /// Gets a reference to the input management system.
    /// </summary>
    public static InputManager Input { get; private set; }

    public static EntityManager Entities { get; private set; }

    public static Scheduler Scheduler { get; } = new Scheduler();
    public static CoreConsole Console { get; } = new CoreConsole();

    public static AudioManager Audio { get; } = new AudioManager();

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }
    public static SpriteFont ConsoleFont { get; set; }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults.
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title.
        Window.Title = title;

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;
        ExitOnEscape = true;

        Window.TextInput += Console.OnTextInput;
    }

    public virtual void ToggleFullScreen()
    {
        Graphics.IsFullScreen = !Graphics.IsFullScreen;

        if (Graphics.IsFullScreen)
        {
            //Graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            //Graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
        } else
        {
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
        }

        Graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        Entities = new EntityManager();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        Input = new InputManager();

        Console.Commands.Register(new HelpCommand(Console.Commands));
        Console.Commands.Register(new DebugUICommand());
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        Console.LoadContent();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        Audio.Dispose();
    }
    protected override void Update(GameTime gameTime)
    {
        Scheduler.Update(gameTime);
        Console.Update(gameTime);

        // Update the input manager.
        Input.Update(gameTime);
        Entities.CheckClicks(Input.Mouse);
        // Check for exit on escape key press.
        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
        {
            Exit();
        }

        if (Input.Keyboard.WasKeyJustReleased(Microsoft.Xna.Framework.Input.Keys.F11))
        {
            ToggleFullScreen();
        }

        Entities.Update(gameTime);
        base.Update(gameTime);

        Audio.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    
        Entities.Draw(SpriteBatch);
        Console.Draw(SpriteBatch);
    }
}
