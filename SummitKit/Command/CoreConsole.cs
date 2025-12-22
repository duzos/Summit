using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SummitKit.Graphics;
using SummitKit.Physics;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

public sealed class CoreConsole : IContentLoader, IUpdating, IDraw
{
    public bool Open { get; set; } = false;
    public CommandManager Commands { get; init; } = new();
    private string input = "";
    private List<ConsoleLine> history = [];
    private List<string> commandHistory = [];
    private int historyIndex = 0;
    private int scrollOffset = 0;
    private const int scrollSpeed = 1;
    private SpriteFont font;
    private Texture2D background;

    public void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!Open) return;


        if (e.Key == Keys.Back && input.Length > 0)
            input = input[..^1];
        else if (e.Key == Keys.Enter)
            SubmitCommand();
        else if (!char.IsControl(e.Character))
            input += e.Character;
    }

    private void SubmitCommand()
    {
        if (!string.IsNullOrWhiteSpace(input)) commandHistory.Add(input);

        historyIndex = commandHistory.Count;
        history.Add(new ConsoleLine("> " + input, Color.Yellow));

        var context = new CommandContext((text, color) =>
        {
            history.Add(new ConsoleLine(text, color));
        });

        try
        {
            Commands.Execute(input, context);
        }
        catch (Exception ex)
        {
            context.Error("Error executing command: " + ex.Message);
        }

        input = "";
        scrollOffset = 0;
    }

    private void HandleHistoryNavigation()
    {
        if (!Open || commandHistory.Count == 0)
            return;

        if (Core.Input.Keyboard.WasKeyJustReleased(Keys.Up))
        {
            historyIndex = Math.Max(0, historyIndex - 1);
            input = commandHistory[historyIndex];
        }

        if (Core.Input.Keyboard.WasKeyJustReleased(Keys.Down))
        {
            historyIndex = Math.Min(commandHistory.Count, historyIndex + 1);

            if (historyIndex == commandHistory.Count)
                input = "";
            else
                input = commandHistory[historyIndex];
        }
    }

    private void HandleScroll()
    {
        int delta = Core.Input.Mouse.ScrollWheelDelta;

        if (delta != 0)
        {
            scrollOffset += Math.Sign(delta) * scrollSpeed;
            scrollOffset = Math.Max(scrollOffset, 0);
        }
    }

    public void LoadContent()
    {
        font = Core.Content.Load<SpriteFont>("assets/balatro");
        background = new Texture2D(Core.GraphicsDevice, 1, 1);
        background.SetData([Color.White]);
    }

    public void Update(GameTime gameTime)
    {
        if (Core.Input.Keyboard.WasKeyJustReleased(Keys.Tab))
        {
            Open = !Open;
        }
        
        if (Open)
        {
            HandleHistoryNavigation();
            HandleScroll();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Open) return;

        int height = Core.GraphicsDevice.Viewport.Height / 3;

        // Grey translucent background
        spriteBatch.Draw(
            background,
            new Rectangle(0, 0, Core.GraphicsDevice.Viewport.Width, height),
            Color.Gray * 0.75f
        );

        int y = 10;

        int visibleLines = (height / font.LineSpacing) - 2;

        int start = Math.Max(0, history.Count - visibleLines - scrollOffset);
        int end = Math.Min(history.Count, start + visibleLines);

        for (int i = start; i < end; i++)
        {
            var line = history[i];

            spriteBatch.DrawString(
                font,
                line.Text,
                new Vector2(10, y),
                line.Color
            );

            y += font.LineSpacing;
        }


        spriteBatch.DrawString(
            font,
            "> " + input + "_",
            new Vector2(10, height - font.LineSpacing - 5),
            Color.Yellow
        );
    }
}