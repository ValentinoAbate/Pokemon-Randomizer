using PokemonRandomizer.Backend.Utilities.Debug;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class TextUtils
    {
        public static string Reformat(string text, char newline, int maxLineLength, int maxLines = 3)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            var tokens = text.Split(' ', newline);
            if (tokens.Length <= 1)
                return text;
            var lines = new List<string>();
            string line = tokens[0];
            for (int i = 1; i < tokens.Length; ++i)
            {
                if(line.Length + tokens[i].Length > maxLineLength)
                {
                    lines.Add(line);
                    line = tokens[i];
                }
                else
                {
                    line += ' ' + tokens[i];
                }
                // Add the last line even if it doesn't overflow
                if(i == tokens.Length - 1)
                {
                    lines.Add(line);
                }
            }
            if(lines.Count > maxLines)
            {
                Logger.main.Warning($"Item Description with greater than {maxLines} detected after reformat ({text})");
            }
            return string.Join(newline.ToString(), lines);
        }
    }
}
