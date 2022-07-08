using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Utilities.Debug
{
    public class Logger
    {
        public enum Level
        {
            Info,
            Todo,
            Warning,
            Unsupported,
            Error,
            StackTrace,
        }

        public static readonly Logger main = new Logger();
        private static readonly LogData emptyData = new LogData("Attempting to access empty log", Level.Error);

        public event Action<LogData> OnLog;

        public int Count => log.Count;
        public IReadOnlyList<LogData> FullLog => log;
        public string[] FullLogText => log.Select(d => d.ToString()).ToArray();
        public LogData LastLog => log.Count > 0 ? log[^1] : emptyData;

        private readonly List<LogData> log = new List<LogData>();

        public void Clear()
        {
            log.Clear();
        }

        public void Log(string message, Level level)
        {
            log.Add(new LogData(message, level));
            OnLog?.Invoke(log[^1]);
        }

        public void Info(string message) => Log(message, Level.Info);

        public void Warning(string message) => Log(message, Level.Warning);

        public void Error(string message) => Log(message, Level.Error);

        public void Unsupported(string message) => Log(message, Level.Unsupported);

        public void Todo(string message) => Log(message, Level.Todo);

        private void Exception(string message, string stackTrace)
        {
            Error(message);
            if(stackTrace != null)
            {
                Log("\n" + stackTrace, Level.StackTrace);
            }
        }

        public void Exception(Exception e)
        {
            Exception(e.Message, e.StackTrace);
        }

        public void Exception(string prefix, Exception e)
        {
            Exception(prefix + e.Message, e.StackTrace);
        }

        public readonly struct LogData
        {
            public readonly string message;
            public readonly Level level;

            public LogData(string message, Level level)
            {
                this.message = message;
                this.level = level;
            }

            public override string ToString()
            {
                return level.ToString() + ": " + message;
            }
        }
    }
}
