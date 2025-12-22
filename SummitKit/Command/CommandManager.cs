using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

public sealed class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    public IEnumerable<ICommand> Commands => _commands.Values;
    public void Register(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (_commands.ContainsKey(command.Name.ToLower()))
        {
            throw new InvalidOperationException($"A command with the name '{command.Name}' is already registered.");
        }
        _commands[command.Name.ToLower()] = command;
    }

    public void RegisterNamespace(string @namespace, Assembly? assembly = null)
    {
        var commandType = typeof(ICommand);
        var asm = assembly ?? Assembly.GetExecutingAssembly();
        var types = asm
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                commandType.IsAssignableFrom(t) &&
                t.Namespace == @namespace
            );

        foreach (var type in types)
        {
            var command = (ICommand)Activator.CreateInstance(type)!;
            Register(command);
        }
    }

    public bool TryGet(string name, out ICommand? command)
    {
        return _commands.TryGetValue(name.ToLower(), out command);
    }
    public void Execute(string input, CommandContext context)
    {
        var parts = input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return;

        var name = parts[0].ToLower();

        if (!_commands.TryGetValue(name, out var command))
        {
            context.Error($"Unknown command: {name}");
            return;
        }

        command.Execute(context, [.. parts.Skip(1)]);
    }
}