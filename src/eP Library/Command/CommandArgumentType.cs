using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    /// <summary>
    /// Represents argument type.
    /// </summary>
    public enum CommandArgumentType
    {
        /// <summary>
        /// Argument that is not start with '-'.
        /// </summary>
        ArgumentString,
        /// <summary>
        /// Argument that starts with '-'.
        /// </summary>
        Option
    }
}
