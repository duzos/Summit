using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summit.Card;
using Summit.Maths;
using Summit.State;
using SummitKit;
using SummitKit.IO;
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
        var manager = SummitKit.Core.SceneManager;
        manager.Register(SummitScene.MainMenu.ToString(), CreateMainMenuScene());
        manager.Register(SummitScene.Gameplay.ToString(), CreateGameplayScene());
        manager.Register(SummitScene.GameOver.ToString(), CreateGameOverScene());

        manager.Current = SummitScene.MainMenu.ToScene();
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
        bool hasSave = ((ISerializable<GameState>)MainGame.State).HasSave();

        UIContainer container = new()
        {
            BackgroundColour = Color.Transparent,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            Spacing = 15,
            Padding = 15
        };
        container.Shadow.Enabled = false;
        container.SetDimensions(400, hasSave ? 370 : 275);
        container.Position = new(screenWidth / 2 - container.Width / 2, screenHeight / 2 - container.Height / 2);
        container.Add();

        // Title text
        var titleText = new UIText(SummitKit.Core.ConsoleFont, MainGame.Name)
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            Spacing = 15
        };
        titleText.SetDimensions(400, 80);
        ((IUIElement)container).AddChild(titleText);
        titleText.Add();


        // Resume button (conditional)
        if (hasSave)
        {
            var resumeText = new UIText(SummitKit.Core.ConsoleFont, "Resume Game")
            {
                VerticalAlign = UIAlign.Center,
                HorizontalAlign = UIAlign.Center,
                TextHorizontalAlign = UIAlign.Center
            };

            var resumeButton = new UIButton(_ =>
            {
                Core.SceneManager.Current = SummitScene.Gameplay.ToScene();
                Scheduler.Delay(() =>
                {
                    MainGame.State.OnLoad();
                }, TransitionDuration);
            })
            {
                Radius = 12,
                Spacing = 15,
                VerticalAlign = UIAlign.Start,
                HorizontalAlign = UIAlign.Center,
            };

            resumeButton.SetDimensions(200, 60);
            resumeText.SetDimensions(resumeButton.PreferredLayout.Size.ToVector2() - new Vector2(resumeButton.Padding));
            resumeButton.BaseColour = Color.CornflowerBlue;
            resumeButton.HoverColour = Color.RoyalBlue;

            ((IUIElement)resumeButton).AddChild(resumeText);
            resumeText.Add();
            resumeButton.Add();
            ((IUIElement)container).AddChild(resumeButton);
        }

        var newText = new UIText(SummitKit.Core.ConsoleFont, "New Game")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };

        var newButton = new UIButton(_ =>
        {
            Core.SceneManager.Current = SummitScene.Gameplay.ToScene();
            Scheduler.Delay(() =>
            {
                MainGame.ResetState(); // deletes save + creates new state + OnLoad()
            }, TransitionDuration);
        })
        {
            Radius = 12,
            Spacing = 15,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
        };

        newButton.SetDimensions(200, 60);
        newText.SetDimensions(newButton.PreferredLayout.Size.ToVector2() - new Vector2(newButton.Padding));
        newButton.BaseColour = Color.Green;
        newButton.HoverColour = Color.DarkGreen;

        ((IUIElement)newButton).AddChild(newText);
        newText.Add();
        newButton.Add();
        ((IUIElement)container).AddChild(newButton);

        var quitText = new UIText(SummitKit.Core.ConsoleFont, "Quit")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };

        var quitButton = new UIButton(_ =>
        {
            Core.Instance.Exit();
        })
        {
            Radius = 12,
            VerticalAlign = UIAlign.End,
            HorizontalAlign = UIAlign.Center,
            Spacing = 15
        };
        quitButton.SetDimensions(200, 60);
        quitText.SetDimensions(quitButton.PreferredLayout.Size.ToVector2() - new Vector2(quitButton.Padding));
        quitButton.Shadow.Enabled = true;
        quitButton.BaseColour = Color.Red;
        quitButton.HoverColour = Color.DarkRed;
        ((IUIElement)quitButton).AddChild(quitText);
        ((IUIElement)container).AddChild(quitButton);
        quitText.Add();
        quitButton.Add();

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

    public static SummitScene? GetSummitScene()
    {
        if (MainGame.SceneManager.Current == null) return null;
        MainGame.SceneManager.TryGetKey(MainGame.SceneManager.Current, out var currentName);
        if (currentName == null) throw new InvalidOperationException("Current scene is not registered in the scene manager.");

        return Enum.TryParse(currentName, out SummitScene scene) ? scene : null;
    }

    public static bool IsCurrentScene(this SummitScene scene)
    {
        var currentScene = GetSummitScene();
        return currentScene.HasValue && currentScene.Value == scene;
    }

    /// <summary>
    /// Stats panel (score, target, etc.)
    /// </summary>
    private static UIContainer CreateStatsSegment()
    {
        UIContainer container = new()
        {
            BackgroundColour = Color.DarkGray,
            Radius = 1,
            Spacing = 15,
            Padding = 15
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
            TextVerticalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = $"{MainGame.State.TargetScore - MainGame.State.ScoreLimits} < X < {MainGame.State.TargetScore + MainGame.State.ScoreLimits}"
        };
        targetScoreText.SetDimensions(targetScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.45F) - new Vector2(targetScore.Padding));
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
            OnUpdate = (t, _) => ((UIText)t).Text = MainGame.State.Score.ToString(2)
        };
        currentScoreText.SetDimensions(currentScore.PreferredLayout.Size.ToVector2() * new Vector2(1, 0.45F) - new Vector2(currentScore.Padding));
        currentScoreText.Add();
        ((IUIElement)currentScore).AddChild(currentScoreText);

        UIContainer roundsPlayed = new()
        {
            VerticalAlign = UIAlign.End,
            HorizontalAlign = UIAlign.Center,
            BackgroundColour = Color.Red
        };
        roundsPlayed.SetDimensions((scores.PreferredLayout.Size.ToVector2() - new Vector2(scores.Padding)) / new Vector2(1.1F, 2.1F));
        ((IUIElement)scores).AddChild(roundsPlayed);
        roundsPlayed.Add();

        UIText roundsPlayedText = new(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            TextVerticalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = $"Round #{MainGame.State.RoundsPlayed}"
        };
        roundsPlayedText.SetDimensions(roundsPlayed.PreferredLayout.Size.ToVector2() - new Vector2(roundsPlayed.Padding));
        roundsPlayedText.Add();
        ((IUIElement)roundsPlayed).AddChild(roundsPlayedText);;

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

    private static Scene CreateGameOverScene()
    {
        var container = CreateGameOverSegment();
        return new Scene([container], TransitionDuration);
    }

    private static UIContainer CreateGameOverSegment()
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
        container.SetDimensions(600, 360);
        container.Position = new(screenWidth / 2 - container.Width / 2, screenHeight / 2 - container.Height / 2);
        container.Add();

        var titleText = new UIText(SummitKit.Core.ConsoleFont, "Game Over")
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        titleText.SetDimensions(400, 80);
        ((IUIElement)container).AddChild(titleText);
        titleText.Add();

        var scoreText = new UIText(SummitKit.Core.ConsoleFont)
        {
            VerticalAlign = UIAlign.Start,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center,
            OnUpdate = (t, _) => ((UIText)t).Text = $"Lost on Round #{MainGame.State.RoundsPlayed}"
        };
        scoreText.SetDimensions(400, 60);
        ((IUIElement)container).AddChild(scoreText);
        scoreText.Add();

        var mainMenuText = new UIText(SummitKit.Core.ConsoleFont, "Main Menu")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        var mainMenuButton = new UIButton(_ =>
        {
            Core.SceneManager.Current = SummitScene.MainMenu.ToScene();

            Scheduler.Delay(() =>
            {
                State.GameState.Reset();
            }, TransitionDuration);
        })
        {
            Radius = 12,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
        };
        mainMenuButton.SetDimensions(200, 60);
        mainMenuText.SetDimensions(mainMenuButton.PreferredLayout.Size.ToVector2() - new Vector2(mainMenuButton.Padding));
        mainMenuButton.Shadow.Enabled = true;
        mainMenuButton.BaseColour = Color.Gray;
        mainMenuButton.HoverColour = Color.DarkGray;
        ((IUIElement)mainMenuButton).AddChild(mainMenuText);
        ((IUIElement)container).AddChild(mainMenuButton);
        mainMenuText.Add();
        mainMenuButton.Add();

        var restartText = new UIText(SummitKit.Core.ConsoleFont, "Restart")
        {
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
            TextHorizontalAlign = UIAlign.Center
        };
        var restartButton = new UIButton(_ =>
        {
            Core.SceneManager.Current = SummitScene.Gameplay.ToScene();

            Scheduler.Delay(() =>
            {
                State.GameState.Reset();
            }, TransitionDuration);
        })
        {
            Radius = 12,
            VerticalAlign = UIAlign.Center,
            HorizontalAlign = UIAlign.Center,
        };
        restartButton.SetDimensions(200, 60);
        restartText.SetDimensions(restartButton.PreferredLayout.Size.ToVector2() - new Vector2(restartButton.Padding));
        restartButton.Shadow.Enabled = true;
        restartButton.BaseColour = Color.Green;
        restartButton.HoverColour = Color.DarkGreen;
        ((IUIElement)restartButton).AddChild(restartText);
        ((IUIElement)container).AddChild(restartButton);
        restartText.Add();
        restartButton.Add();

        ((IUIElement)container).RecalculateLayout();

        return container;
    }
}