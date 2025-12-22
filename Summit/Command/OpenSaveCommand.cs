using Summit.State;
using SummitKit.Command;
using SummitKit.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class OpenSaveCommand : ICommand
{
    public string Name => "open-save";
    public string Usage => "open-save";
    public string Description => "Opens the game state save file.";
    public void Execute(CommandContext ctx, string[] args)
    {
        string path = ((ISerializable<GameState>)MainGame.State).FilePath;
        
        if (!File.Exists(path))
        {
            ctx.Error($"Save file does not exist at path: {path}");
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
            ctx.Success($"Opened save file at path: {path}");
        }
        catch (Exception ex)
        {
            ctx.Error($"Failed to open save file: {ex.Message}");
        }
    }
}