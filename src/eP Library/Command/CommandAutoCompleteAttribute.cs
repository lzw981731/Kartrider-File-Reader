using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    /// <summary>
    /// Indicates that a method can be used to auto complete a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAutoCompleteAttribute: Attribute
    {
        /// <summary>
        /// The name of command for which this method can give any auto complete recommend. 
        /// </summary>
        public string CommandName { get; init; }
        /// <summary>
        /// Initializes a <see cref="CommandAutoCompleteAttribute"/> instance.
        /// </summary>
        /// <param name="commandName">The name of command for which this method can give any auto complete recommend.</param>
        public CommandAutoCompleteAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
