using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    /// <summary>
    /// Indicates that a method can be regarded as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute: Attribute
    {
        /// <summary>
        /// The name of a command.
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// The description of a command. It will display on help.
        /// </summary>
        public string CommandDescription { get; set; }

        /// <summary>
        /// Initializes a <see cref="CommandArgument"/> instance.
        /// </summary>
        /// <param name="commandName">The name of a command.</param>
        /// <param name="desc">The description of a command.</param>
        public CommandAttribute(string commandName, string desc = "") 
        { 
            CommandName = commandName;
            CommandDescription = desc;
        }
    }
}
