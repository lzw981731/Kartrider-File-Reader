using System.Collections.Concurrent;
using System.Drawing;

namespace eP.Logging
{
    public static class ConsolePrinter
    {
        private static readonly ConcurrentQueue<ConsoleText> TextQueue = new ConcurrentQueue<ConsoleText>();

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        private static Task? _activeUpdateWorker;

        private static Mutex _mutex = new Mutex();

        private static bool _requiredStop = false;
        private static bool _stopped = false;
        
        public static void WriteLine(string prefix,string message,ConsoleColor color)
        {
            try
            {
                string[] lines = message.Split('\n');
                foreach (var line in lines)
                {
                    ConsoleText consoleText = new ConsoleText
                    {
                        Color = color,
                        Text = $"[{prefix}] {line}"
                    };
                    TextQueue.Enqueue(consoleText);
                }

                try
                {
                    _mutex.WaitOne();

                    if (_requiredStop)
                    {
                        return;
                    }
                    
                    if ((_activeUpdateWorker?.IsCompleted ?? false) ||
                        (_activeUpdateWorker?.IsFaulted ?? false) ||
                        (_activeUpdateWorker?.IsCanceled ?? false))
                        _activeUpdateWorker = null;
                    
                    _activeUpdateWorker ??= new Task(UpdateTextWorker);
                    
                    if (_activeUpdateWorker.Status == TaskStatus.Created)
                        _activeUpdateWorker.Start();
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public static void PrintInfo(string message)
        {
            WriteLine("Info", message, ConsoleColor.White);
        }

        public static void PrintWarning(string message)
        {
            WriteLine("Warning", message, ConsoleColor.Yellow);
        }

        public static void PrintError(string message)
        {
            WriteLine("Error", message, ConsoleColor.Red);
        }

        public static void PrintDebug(string message)
        {
            WriteLine("Debug", message, ConsoleColor.Cyan);
        }

        private static void UpdateTextWorker()
        {
            DateTime previousUpdateTime =  DateTime.Now;
            TimeSpan previousUpdateInterval = DateTime.Now - previousUpdateTime;
            while (previousUpdateInterval.Milliseconds <= 1000)
            {
                while (TextQueue.TryDequeue(out var queueObj))
                {
                    if(queueObj.Color != ConsoleColor.White)
                        Console.ForegroundColor = queueObj.Color;
                    
                    Console.WriteLine(queueObj.Text);
                    Console.ResetColor();
                    
                    previousUpdateTime =  DateTime.Now;
                    previousUpdateInterval = DateTime.Now - previousUpdateTime;
                }

                if (CancellationTokenSource.IsCancellationRequested)
                    break;
                
                Thread.Sleep(5);
            }
            
            _stopped = true;
        }

        public static void RequireStop()
        {
            try
            {
                _mutex.WaitOne();
                _requiredStop = true;
                CancellationTokenSource.Cancel();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            _activeUpdateWorker?.Wait(1500);
        }
    }

    public struct ConsoleText
    {
        public ConsoleColor Color;

        public string Text;

        public string ConvertToCssColor()
        {
            Color a = ConsoleColorToColor(Color);
            return $"#{Convert.ToString((int)a.R, 16).PadLeft(2, '0')}{Convert.ToString((int)a.G, 16).PadLeft(2, '0')}{Convert.ToString((int)a.B, 16).PadLeft(2, '0')}";
        }

        private Color ConsoleColorToColor(ConsoleColor color)
        {
            int baseColor = (int)color;
            if (baseColor == 7)
            {
                return System.Drawing.Color.FromArgb(192, 192, 192);
            }
            else if (baseColor == 8)
            {
                return System.Drawing.Color.FromArgb(128, 128, 128);
            }
            int r = ((baseColor >> 2) & 1) << (7 + ((baseColor >> 3) & 1));
            int g = ((baseColor >> 1) & 1) << (7 + ((baseColor >> 3) & 1));
            int b = ((baseColor >> 0) & 1) << (7 + ((baseColor >> 3) & 1));
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}
