using Summit.State;
using SummitKit.Command;
using SummitKit.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class LoadCommand : ICommand
{
    public string Name => "load";
    public string Usage => "load";
    public string Description => "Loads the current game state from the file.";
    public void Execute(CommandContext ctx, string[] args)
    {
        ((ISerializable<GameState>)MainGame.State).Load();
        ctx.Success("Game state loaded successfully.");
    }
}