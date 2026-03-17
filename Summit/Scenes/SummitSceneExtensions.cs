using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summit.Card;
using SummitKit.Physics;
using SummitKit.UI;
using SummitKit.UI.Scene;
using SummitKit.Util;
using System;
using System.Collections.Generic;

namespace Summit.Scenes;

public static class SummitSceneExtensions
{
    private static readonly TimeSpan TransitionDuration = TimeSpan.FromSeconds(0.25);

    public static string ToFriendlyString(this SummitScene scene)
    {
        return scene switch
        {
            SummitScene.MainMenu => "Main Menu",
            SummitScene.Gameplay => "Gameplay",
            SummitScene.GameOver => "Game Over",
            _ => scene.ToString()
        };
    }

    public static Scene? ToScene(this SummitScene scene)
    {
        return SummitKit.Core.SceneManager.Get(scene.ToString());
    }

    public static void TransitionTo(this SummitScene scene)
    {
        SummitKit.Core.SceneManager.Current = scene.ToScene();
    }

    public static void Initialize()
    {
        SummitKit.Core.SceneManager.Register(SummitScene.MainMenu.ToString(), CreateMainMenuScene());
        SummitKit.Core.SceneManager.Register(SummitScene.Gameplay.ToString(), CreateGameplayScene());

        // Disable gameplay scene initially (move elements off-screen)
        SummitScene.Gameplay.ToScene()?.Disable();

        // Start with MainMenu enabled
        SummitScene.MainMenu.ToScene()?.Enable();
    }

    private static Scene CreateMainMenuScene()
    {
        var titleContainer = CreateTitleSegment();
        return new Scene([titleContainer], TransitionDuration);
    }

    private static UIContainer CreateTitleSegment()
    {
        var screenWidth = SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferWidth;
        var screenHeight = SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferHeight;

        UIContainer container = new()
        {
            BackgroundColour = Color.Transparent,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
        };
        container.Shadow.Enabled = false;
        container.SetDimensions(400, 300);
        container.Position = new(screenWidth / 2 - container.Width / 2, screenHeight / 2 - container.Height / 2);
        container.Add();

        // Title text
        var titleText = new UIText(SummitKit.Core.ConsoleFont, MainGame.Name)
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        titleText.SetDimensions(400, 80);
        ((IUIElement)container).AddChild(titleText);
        titleText.Add();

        // Play button
        var playText = new UIText(SummitKit.Core.ConsoleFont, "Play")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };

        var playButton = new UIButton(_ =>
             {
                 // Transition to gameplay
                 var mainMenu = SummitScene.MainMenu.ToScene();
                 var gameplay = SummitScene.Gameplay.ToScene();
                 mainMenu?.Transition(gameplay);

                 // Start the game after transition
                 Scheduler.Delay(() =>
                {
                     MainGame.State.NextRound();
                 }, TransitionDuration);
             })
        {
            Radius = 12,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
        };
        playButton.SetDimensions(200, 60);
        playText.SetDimensions(playButton.PreferredLayout.Size.ToVector2() - new Vector2(playButton.Padding));
        playButton.Shadow.Enabled = true;
        playButton.BaseColour = Color.Green;
        playButton.HoverColour = Color.DarkGreen;
        ((IUIElement)playButton).AddChild(playText);
        ((IUIElement)container).AddChild(playButton);
        playText.Add();
        playButton.Add();

        ((IUIElement)container).RecalculateLayout();

        return container;
    }

    private static Scene CreateGameplayScene()
    {
        var statsSegment = CreateStatsSegment();
        var playSegment = CreatePlaySegment();

        // Use the actual Hand from GameState
        List<IPositioned> elements = [statsSegment, playSegment, MainGame.State.MainHand];
        return new Scene(elements, TransitionDuration);
    }

    /// <summary>
    /// Stats panel (score, target, etc.)
    /// </summary>
    private static UIContainer CreateStatsSegment()
    {
        UIContainer container = new()
        {
            BackgroundColour = Color.DarkGray,
            Radius = 1
        };
        container.Shadow.Enabled = false;
        container.CollidesWithWindowEdges = false;
        container.HasCollisions = false;
        container.SetDimensions(300, SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferHeight);
        container.Position = new(15, SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - container.Height / 2);
        container.Add();

        // Container to show scores
        UIContainer scores = new()
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            BackgroundColour = Color.Transparent
        };
        scores.SetDimensions((container.PreferredLayout.Size.ToVector2() - new Vector2(container.Padding)) / new Vector2(1.1F, 2.1F));
        ((IUIElement)container).AddChild(scores);
        scores.Add();

        // Target score container
        UIContainer targetScore = new()
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            BackgroundColour = Color.Orange
        };
        targetScore.SetDimensions((scores.PreferredLayout.Size.ToVector2() - new Vector2(scores.Padding)) / new Vector2(1.1F, 2.1F));
        ((IUIElement)scores).AddChild(targetScore);
        targetScore.Add();

        UIText targetScoreText = new(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = "Target"
        };
        targetScoreText.SetDimensions(targetScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.5F) - new Vector2(targetScore.Padding));
        targetScoreText.Add();
        ((IUIElement)targetScore).AddChild(targetScoreText);

        targetScoreText = new(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.End,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = $"{MainGame.State.TargetScore - MainGame.State.ScoreLimits} < X < {MainGame.State.TargetScore + MainGame.State.ScoreLimits}"
        };
        targetScoreText.SetDimensions(targetScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.5F) - new Vector2(targetScore.Padding));
        targetScoreText.Add();
        ((IUIElement)targetScore).AddChild(targetScoreText);

        // Current score container
        UIContainer currentScore = new()
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            BackgroundColour = Color.Green
        };
        currentScore.SetDimensions((scores.PreferredLayout.Size.ToVector2() - new Vector2(scores.Padding)) / new Vector2(1.1F, 2.1F));
        ((IUIElement)scores).AddChild(currentScore);
        currentScore.Add();

        UIText currentScoreText = new(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = "Score"
        };
        currentScoreText.SetDimensions(currentScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.5F) - new Vector2(currentScore.Padding));
        currentScoreText.Add();
        ((IUIElement)currentScore).AddChild(currentScoreText);

        currentScoreText = new(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.End,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = MainGame.State.Score.ToString()
        };
        currentScoreText.SetDimensions(currentScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.5F) - new Vector2(currentScore.Padding));
        currentScoreText.Add();
        ((IUIElement)currentScore).AddChild(currentScoreText);

        return container;
    }

    private static UIContainer CreatePlaySegment()
    {
        UIContainer container = new()
        {
            BackgroundColour = Color.Transparent
        };
        container.Shadow.Enabled = false;
        container.SetDimensions(520, 100);
        container.Position = new(SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - container.Width / 2, SummitKit.Core.GraphicsDevice.PresentationParameters.BackBufferHeight - 15 - container.Height);
        container.Add();

        var text = new UIText(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = "Play (" + MainGame.State.RemainingHands + ")"
        };

        var button = new UIButton(_ =>
        {
            MainGame.State.PlaySelected();
            MainGame.State.Deal();
        });
        button.SetDimensions(200, 100);
        text.SetDimensions(button.PreferredLayout.Size.ToVector2() - new Vector2(button.Padding));
        button.Shadow.Enabled = true;
        button.BaseColour = Color.Blue;
        button.HoverColour = Color.DarkBlue;
        button.OnUpdate = (b, _) => ((UIButton)b).Enabled = MainGame.State.RemainingHands > 0;
        ((IUIElement)button).AddChild(text);
        text.Add();
        button.Add();
        ((IUIElement)container).AddChild(button);

        var sortContainer = new UIContainer()
        {
            BackgroundColour = Color.Transparent,
            Padding = 0,
            Spacing = 10
        };
        sortContainer.SetDimensions(100, 100);
        sortContainer.Shadow.Enabled = false;

        var sortText = new UIText(SummitKit.Core.ConsoleFont, "Rank")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        button = new UIButton(_ =>
    {
        MainGame.State.MainHand.SortCards(Hand.SortByValue);
        MainGame.State.LastSort = Hand.SortByValue;
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

        sortText = new UIText(SummitKit.Core.ConsoleFont, "Suit")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        button = new UIButton(_ =>
             {
                 MainGame.State.MainHand.SortCards(Hand.SortBySuit);
                 MainGame.State.LastSort = Hand.SortBySuit;
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

        text = new UIText(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = "Discard (" + MainGame.State.RemainingDiscards + ")"
        };

        button = new UIButton(_ => MainGame.State.DiscardSelected());
        button.SetDimensions(200, 100);
        text.SetDimensions(button.PreferredLayout.Size.ToVector2() - new Vector2(button.Padding));
        button.Shadow.Enabled = true;
        button.BaseColour = Color.Red;
        button.HoverColour = Color.DarkRed;
        button.OnUpdate = (b, _) => ((UIButton)b).Enabled = MainGame.State.RemainingDiscards > 0;
        ((IUIElement)button).AddChild(text);
        text.Add();
        button.Add();
        ((IUIElement)container).AddChild(button);

        ((IUIElement)container).RecalculateLayout();

        return container;
    }
}