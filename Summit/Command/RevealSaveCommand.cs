using Summit.State;
using SummitKit.Command;
using SummitKit.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Summit.Command;

public sealed class RevealSaveCommand : ICommand
{
    public string Name => "reveal-save";
    public string Usage => "reveal-save";
    public string Description => "Opens the game state save file directory in file explorer";
    public void Execute(CommandContext ctx, string[] args)
    {
        string path = ((ISerializable<GameState>)MainGame.State).FilePath;

        if(!File.Exists(path))
        {
            ctx.Error($"File not found: {path}");
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: open folder and select file
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: open folder and select file
            Process.Start("open", $"-R \"{path}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: open folder (cannot always highlight file)
            var folder = Path.GetDirectoryName(path) ?? "/";
            Process.Start("xdg-open", folder);
        }
        else
        {
            ctx.Error("Unsupported OS for RevealInExplorer.");
            return;
        }

        ctx.Success($"Revealed save file location: {path}");
    }
}