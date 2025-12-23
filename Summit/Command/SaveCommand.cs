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

public sealed class SaveCommand : ICommand
{
    public string Name => "save";
    public string Usage => "save [delete | open | reveal]";
    public void Execute(CommandContext ctx, string[] args)
    {
        if (args.Length > 0)
        {
            string path = ((ISerializable<GameState>)MainGame.State).FilePath;

            switch (args[0])
            {
                case "open":
                    {

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
                        return;
                    }
                case "reveal":

                    if (!File.Exists(path))
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
                    return;
                case "delete":
                    {
                                               if (!File.Exists(path))
                        {
                            ctx.Error($"Save file does not exist at path: {path}");
                            return;
                        }
                        try
                        {
                            File.Delete(path);
                            ctx.Success("Save file deleted successfully.");
                        }
                        catch (Exception ex)
                        {
                            ctx.Error($"Failed to delete save file: {ex.Message}");
                        }
                        return;
                    }
                default:
                    {
                        ctx.Error($"Unknown argument: {args[0]}");
                        return;
                    }
            }
        }

        ((ISerializable<GameState>)MainGame.State).Save();
        ctx.Success("Game state saved successfully.");
    }
}