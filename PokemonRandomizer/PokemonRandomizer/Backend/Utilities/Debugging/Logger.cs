﻿using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities.Debug
{
    public class Logger
    {
        public enum Level
        {
            Info,
            Warning,
            Unsupported,
            Error,
        }

        public static readonly Logger main = new Logger();
        private static readonly LogData emptyData = new LogData("Attempting to access empty log", Level.Error);

        public event Action<LogData> OnLog;

        public int Count => log.Count;
        public IEnumerable<LogData> FullLog => log;
        public LogData LastLog => log.Count > 0 ? log[log.Count - 1] : emptyData;

        private readonly List<LogData> log = new List<LogData>();

        public void Clear()
        {
            log.Clear();
        }

        public void Log(string message, Level level)
        {
            log.Add(new LogData(message, level));
            OnLog?.Invoke(log[log.Count - 1]);
        }

        public void Info(string message) => Log(message, Level.Info);

        public void Warning(string message) => Log(message, Level.Warning);

        public void Error(string message) => Log(message, Level.Error);

        public void Unsupported(string message) => Log(message, Level.Unsupported);

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
