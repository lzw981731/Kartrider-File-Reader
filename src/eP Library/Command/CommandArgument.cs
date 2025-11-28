using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    /// <summary>
    /// Represents a scanned argument.
    /// </summary>
    /// <param name="ArgumentType">The argument type.</param>
    /// <param name="Value">The original value of argument.</param>
    public record struct CommandArgument(CommandArgumentType ArgumentType, string Value);
}
