﻿using PokemonRandomizer.Backend.Utilities.Debug;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class TextUtils
    {
        public static string Reformat(string text, char newline, int maxLineLength) => Reformat(text, newline, maxLineLength, out int _);
        public static string Reformat(string text, char newline, int maxLineLength, out int numLines)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                numLines = 0;
                return text;
            }
            var tokens = text.Replace(newline + "-", "-").Replace("-" + newline, "-").Split(' ', newline);
            if (tokens.Length <= 1)
            {
                numLines = tokens.Length;
                return text;
            }
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
            numLines = lines.Count;
            return string.Join(newline.ToString(), lines);
        }

        public static string ReformatAndAbbreviate(string text, char newline, int maxLineLength, int maxLines, string[] toRemove, (string, string)[] toReplace)
        {
            string reformatted = Reformat(text, newline, maxLineLength, out int lines);
            if (lines <= maxLines)
                return reformatted;
            Logger.main.Info($"Text ({RemoveNewLines(text)}) with greater than {maxLines} lines ({lines}) detected after reformat. Attempting abbreviation");
            string abbreviated = text.Replace(newline, ' ');
            if(toRemove.Length > 0)
            {
                // Remove uneccesary words
                abbreviated = RemoveAll(abbreviated, toRemove);
            }
            if(toReplace.Length > 0)
            {
                // try replacements
                abbreviated = ReplaceAll(abbreviated, toReplace);
            }
            reformatted = Reformat(abbreviated, newline, maxLineLength, out lines);
            if (lines <= maxLines)
            {
                Logger.main.Info($"Reformat successful, text ({RemoveNewLines(reformatted)}) is now {lines} lines");
                return reformatted;
            }
            Logger.main.Warning($"Reformat failed, text ({RemoveNewLines(reformatted)}) is still {lines} lines");
            return reformatted;
        }

        private static string RemoveNewLines(string text) => text.Replace('\n', ' ');

        private static string RemoveAll(string input, params string[] tokensToRemove)
        {
            string abbreviated = input;
            foreach (var token in tokensToRemove)
            {
                abbreviated = abbreviated.Replace(token, string.Empty);
            }
            return abbreviated;
        }

        private static string ReplaceAll(string input, params (string original, string replacement)[] tokensToReplace)
        {
            string abbreviated = input;
            foreach (var token in tokensToReplace)
            {
                abbreviated = abbreviated.Replace(token.original, token.replacement);
            }
            return abbreviated;
        }
    }
}
