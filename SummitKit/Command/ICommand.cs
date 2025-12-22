using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummitKit.Command;

public interface ICommand
{
    string Name { get; }
    string? Description => null;
    string Usage { get; }
    void Execute(CommandContext context, string[] args);
}