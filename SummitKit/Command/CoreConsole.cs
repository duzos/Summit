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
using static System.Net.Mime.MediaTypeNames;

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

    public CommandContext Context => new((text, color) =>
    {
        history.Add(new ConsoleLine(text, color));
    });

    private void SubmitCommand()
    {
        if (!string.IsNullOrWhiteSpace(input)) commandHistory.Add(input);

        historyIndex = commandHistory.Count;
        history.Add(new ConsoleLine("> " + input, Color.Yellow));

        try
        {
            Commands.Execute(input, Context);
        }
        catch (Exception ex)
        {
            Context.Error("Error executing command: " + ex.Message);
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

    private List<string> WordWrap(string text, float? maxWidthNull = null, SpriteFont? fontNull = null)
    {
        float maxWidth = maxWidthNull.GetValueOrDefault(Core.GraphicsDevice.Viewport.Width + 10);
        SpriteFont font1 = fontNull ?? font;

        var lines = new List<string>();

        foreach (var paragraph in text.Split('\n'))
        {
            string currentLine = "";
            foreach (var word in paragraph.Split(' '))
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                if (font1.MeasureString(testLine).X > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);
        }

        return lines;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Open) return;

        int height = Core.GraphicsDevice.Viewport.Height / 3;

        // Grey translucent background
        spriteBatch.Draw(
            background,
            new Rectangle(0, 0, Core.GraphicsDevice.Viewport.Width, height),
            null,
            Color.Gray * 0.75f,
            0,
            Vector2.Zero,
            SpriteEffects.None,
            0.99F
        );

        int y = 10;

        int visibleLines = (height / font.LineSpacing) - 2;

        List<ConsoleLine> historyWrap = [];

        foreach (var line in history) { 
            foreach (var wrapLine in WordWrap(line.Text))
            {
                historyWrap.Add(new ConsoleLine(wrapLine, line.Color));
            }
        }

        int start = Math.Max(0, historyWrap.Count - visibleLines - scrollOffset);
        int end = Math.Min(historyWrap.Count, start + visibleLines);

        for (int i = start; i < end; i++)
        {
            var line = historyWrap[i];
            spriteBatch.DrawString(
                font,
                line.Text,
                new Vector2(10, y),
                line.Color,
                0.0F,
                Vector2.Zero,
                1,
                SpriteEffects.None,
                1
            );
            y += font.LineSpacing;

        }


        spriteBatch.DrawString(
            font,
            "> " + input + "_",
            new Vector2(10, height - font.LineSpacing - 5),
            Color.Yellow,
            0.0F,
            Vector2.Zero,
            1,
            SpriteEffects.None,
            1
        );
    }
}