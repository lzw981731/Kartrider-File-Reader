using eP.Command.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    /// <summary>
    /// Represents a queue for scanned arguments.
    /// </summary>
    public class CommandArgumentQueue
    {
        private Queue<CommandArgument> _rawArgumentQueue = new Queue<CommandArgument>();
        private int _argumentNum = 0;

        public CommandArgumentType CommandArgumentType => _rawArgumentQueue.Count > 0 ? _rawArgumentQueue.Peek().ArgumentType : throw new Exception();
        
        public int Count => _rawArgumentQueue.Count;


        public CommandArgumentQueue() 
        { 

        }

        public string PopOption()
        {
            CommandArgument poppedArgument = Pop();
            if (poppedArgument.ArgumentType != CommandArgumentType.Option)
                throw new CommandArgumentException(_argumentNum, CommandArgumentExceptionType.RequiredArgumentIsNotOption, "");
            return poppedArgument.Value;
        }

        public string PopArgumentString()
        {
            CommandArgument poppedArgument = Pop();
            if (poppedArgument.ArgumentType != CommandArgumentType.ArgumentString)
                throw new CommandArgumentException(_argumentNum, CommandArgumentExceptionType.RequiredArgumentIsNotOption, "");
            return poppedArgument.Value;
        }

        public CommandArgument Pop()
        {
            if (_rawArgumentQueue.Count <= 0)
                throw new CommandArgumentException(_argumentNum, CommandArgumentExceptionType.ArgumentQueueIsEmpty, "");
            _argumentNum++;
            return _rawArgumentQueue.Dequeue();
        }

        internal void push(CommandArgument arg)
        {
            _rawArgumentQueue.Enqueue(arg);
        }
    }
}
