using SummitKit.Command;
using SummitKit.UI.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.UI.Scenes;

public sealed class CommandScene : ICommand
{
    public string Name => "scene";
    public string Usage => "scene [index]";

    public void Execute(CommandContext ctx, string[] args)
    {
        var manager = Core.SceneManager;

        if (args.Length != 1)
        {
            ctx.Reply("Available scenes:");

            foreach (var key in manager.Keys)
            {
                ctx.Reply($"- {key}");
            }

            return;
        }

        var name = args[0];

        if (!manager.TryGet(name, out var scene))
        {
            ctx.Error($"Scene '{name}' not found.");
            return;
        }

        manager.Current = scene;
        ctx.Success($"Switched to scene '{name}'.");
    }
}