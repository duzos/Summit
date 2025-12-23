using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

public sealed class HelpCommand : ICommand
{
    private readonly CommandManager _manager;

    public HelpCommand(CommandManager manager)
    {
        _manager = manager;
    }

    public string Name => "help";
    public string Usage => "help [command]";

    public void Execute(CommandContext ctx, string[] args)
    {
        if (args.Length == 0)
        {
            ctx.Reply("Available commands:");

            foreach (var cmd in _manager.Commands.OrderBy(c => c.Name))
                ctx.Reply($"  {cmd.Name}" + (cmd.Description is null ? "" : " - " + cmd.Description));
            return;
        }

        var name = args[0].ToLower();

        if (!_manager.TryGet(name, out var command))
        {
            ctx.Error("Unknown command");
            return;
        }

        if (command.Description is not null)
            ctx.Reply(command.Description);
        ctx.Reply(command.Usage);
    }
}