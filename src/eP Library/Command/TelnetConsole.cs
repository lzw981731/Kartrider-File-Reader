using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace eP.Command
{
    public class TelnetConsole : IConsole
    {
        private Lock _lockLock = new Lock();
        private bool _locked = false;
        private int _lockThreadId = -1;

        public int Width => Console.WindowWidth;

        public int Height => Console.WindowHeight;

        public int CursorX => Console.CursorLeft;

        public int CursorY => Console.CursorTop;

        public event Func<string, int, string[]> AutoComplete;

        public char[] Separators {  get; set; } = new char[0];

        private Socket _baseSocket;
        private NetworkStream _ns;
        private StreamReader _strReader;
        private StreamWriter _strWriter;
        private byte[] _buffer;
        
        public TelnetConsole(Socket baseSocket)
        {
            _baseSocket = baseSocket;
            _ns = new NetworkStream(_baseSocket);
            _strReader = new StreamReader(_ns);
            _strWriter = new StreamWriter(_ns);
            
            // System.ReadLine.HistoryEnabled = true;
            // System.ReadLine.AutoCompletionHandler = new TelnetConsoleAutoCompleteHandler(this);
        }
        
        public void Clear()
        {
            Write("\e[3J");
        }
        
        public void MoveCursorTo(int x, int y)
        {
            Write($"\e[{x};{y}H");
        }
        
        public char ReadChar()
        {
            byte[] buffer = [0];
            if (_baseSocket.Receive(buffer) == buffer.Length)
                return (char)buffer[0];
            else
                return '\0';
        }

        public ConsoleKeyInfo ReadKey()
        {
            throw new NotSupportedException();
        }
        
        public string? ReadLine()
        {
            return _strReader.ReadLine();
        }

        public void SetBackgroundColor(Color color)
        {
            Write($"\e[48;2;{color.R};{color.G};{color.B}m");
        }
        
        public void SetBackgroundColor(ConsoleColor color)
        {
            string str = color switch
            {
                ConsoleColor.Black       => "\e[40m",
                ConsoleColor.DarkRed     => "\e[41m",
                ConsoleColor.DarkGreen   => "\e[42m",
                ConsoleColor.DarkYellow  => "\e[43m",
                ConsoleColor.DarkBlue    => "\e[44m",
                ConsoleColor.DarkMagenta => "\e[45m",
                ConsoleColor.DarkCyan    => "\e[46m",
                ConsoleColor.DarkGray    => "\e[47m",
                ConsoleColor.Gray        => "\e[100m",
                ConsoleColor.Red         => "\e[101m",
                ConsoleColor.Green       => "\e[102m",
                ConsoleColor.Yellow      => "\e[103m",
                ConsoleColor.Blue        => "\e[104m",
                ConsoleColor.Magenta     => "\e[105m",
                ConsoleColor.Cyan        => "\e[106m",
                ConsoleColor.White       => "\e[107m",
            };
            Write(str);
        }

        public void SetForegroundColor(Color color)
        {
            Write($"\u001b[38;2;{color.R};{color.G};{color.B}m");
        }

        public void SetForegroundColor(ConsoleColor color)
        {
            string str = color switch
            {
                ConsoleColor.Black       => "\e[30m",
                ConsoleColor.DarkRed     => "\e[31m",
                ConsoleColor.DarkGreen   => "\e[32m",
                ConsoleColor.DarkYellow  => "\e[33m",
                ConsoleColor.DarkBlue    => "\e[34m",
                ConsoleColor.DarkMagenta => "\e[35m",
                ConsoleColor.DarkCyan    => "\e[36m",
                ConsoleColor.DarkGray    => "\e[37m",
                ConsoleColor.Gray        => "\e[90m",
                ConsoleColor.Red         => "\e[91m",
                ConsoleColor.Green       => "\e[92m",
                ConsoleColor.Yellow      => "\e[93m",
                ConsoleColor.Blue        => "\e[94m",
                ConsoleColor.Magenta     => "\e[95m",
                ConsoleColor.Cyan        => "\e[96m",
                ConsoleColor.White       => "\e[97m",
            };
            Write(str);
        }

        public void SetDefaultColor()
        {
            Write("\e[0m");
        }

        public void Write(string message)
        {
            _strWriter.Write(message);
        }

        public void Write(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args));
        }

        public void WriteLine(string message)
        {
            Write(message + '\n');
        }

        public void WriteLine(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args) + '\n');
        }

        public void SetLock()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            while (_locked && currentThreadId != _lockThreadId) ;
            lock (_lockLock)
            {
                _lockThreadId = currentThreadId;
                _locked = true;
            }
        }

        public void ReleaseLock()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (currentThreadId != _lockThreadId)
                return;
            _lockThreadId = -1;
            _locked = false ;
        }

        internal string[] OnAutoComplete(string text, int index)
        {
            return AutoComplete?.Invoke(text, index) ?? new string[0];
        }

        private class TelnetConsoleAutoCompleteHandler : IAutoCompleteHandler
        {
            public char[] Separators
            {
                get => _telnetConsole.Separators;
                set => _telnetConsole.Separators = value;
            }

            private TelnetConsole _telnetConsole;

            public TelnetConsoleAutoCompleteHandler(TelnetConsole telnetConsole)
            {
                _telnetConsole = telnetConsole;
            }

            public string[] GetSuggestions(string text, int index)
            {
                return _telnetConsole.OnAutoComplete(text, index);
            }
        }
    }
}
