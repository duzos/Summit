using Summit.State;
using SummitKit.Command;
using SummitKit.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class SaveCommand : ICommand
{
    public string Name => "save";
    public string Usage => "save";
    public string Description => "Saves the current game state to a file.";
    public void Execute(CommandContext ctx, string[] args)
    {
        ((ISerializable<GameState>)MainGame.State).Save();
        ctx.Success("Game state saved successfully.");
    }
}