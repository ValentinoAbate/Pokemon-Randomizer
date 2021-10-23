using System;
using System.Diagnostics;

namespace PokemonRandomizer.Backend.Utilities.Debug
{

    public class Timer
    {
        public static readonly Timer main = new Timer();

        private readonly Stopwatch stopwatch = new Stopwatch();

        [Conditional("DEBUG")]
        public void Start()
        {
            stopwatch.Restart();
        }

        [Conditional("DEBUG")]
        public void Stop()
        {
            stopwatch.Stop();
        }

        [Conditional("DEBUG")]
        public void Log(string name)
        {
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopwatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Logger.main.Info($"{name} Time: {elapsedTime}");
        }
    }
}
