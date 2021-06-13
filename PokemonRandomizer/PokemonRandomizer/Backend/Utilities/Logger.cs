using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class Logger
    {
        public enum Level
        {
            Info,
            Warning,
            Error,
        }

        private static readonly List<LogData> log = new List<LogData>();

        public static IEnumerable<string> LogReadout => log.Select(d => d.ToString());

        public static void Clear()
        {
            log.Clear();
        }

        public static void Log(string message, Level level)
        {
            log.Add(new LogData(message, level));
        }

        public static void Info(string message) => Log(message, Level.Info);

        public static void Warning(string message) => Log(message, Level.Warning);

        public static void Error(string message) => Log(message, Level.Error);

        public struct LogData
        {
            public string message;
            public Level level;

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
