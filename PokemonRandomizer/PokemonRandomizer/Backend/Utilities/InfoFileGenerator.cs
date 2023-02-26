using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokemonRandomizer.Backend.Utilities
{
    public class InfoFileGenerator
    {
        private const string divider = "===============================================================================================================================================";
        private const string subdivider = "---------------------------------------------------------------------------------------------------------------------------------------";
        private const string unrandomized = "None (unrandomized)";
        private const string none = "None";
        private const int headerLines = 5;
        public string[] GenerateInfoFile(RomData data, RomMetadata metadata, string settingsString = null)
        {
            var lines = new List<string>((data.Pokemon.Count * 9) + (data.RandomizationResults.Count * 4) + (headerLines * 6) + 5);
            Header(ref lines, "Randomizer Info");
            lines.Add($"Randomizer Version : {MainWindow.version}");
            lines.Add($"Seed               : {(string.IsNullOrEmpty(data.Seed) ? unrandomized : data.Seed)}");
            lines.Add($"ROM                : {metadata}");
            lines.Add($"Generation         : {metadata.Gen}");

            if (!string.IsNullOrEmpty(settingsString))
            {
                Header(ref lines, "Settings String");
                lines.Add(settingsString);
            }

            if (data.RandomizationResults.Count > 0)
            {
                Header(ref lines, "Randomization Results", false);
                foreach(var kvp in data.RandomizationResults)
                {
                    SubHeader(ref lines, kvp.Key);
                    lines.Add(string.Join(", ", kvp.Value));
                }
            }

            GenerateTypeChart(data.Types, data.TypeDefinitions, ref lines);

            GeneratePokemonInfo(data, ref lines);

            return lines.ToArray();

        }

        private const string indent = "   ";
        private static void Header(ref List<string> lines, string text, bool emptyLineAfterHeader = true)
        {
            lines.Add(string.Empty);
            lines.Add(divider);
            lines.Add(indent + text);
            lines.Add(divider);
            if (emptyLineAfterHeader)
            {
                lines.Add(string.Empty);
            }
        }

        private static void SubHeader(ref List<string> lines, string text)
        {
            lines.Add(string.Empty);
            lines.Add(text);
            lines.Add(subdivider);
        }

        private static void GenerateTypeChart(List<PokemonType> types, TypeEffectivenessChart chart, ref List<string> lines)
        {
            Header(ref lines, "Type Effectiveness Chart");

            lines.Add(" DEF | ATK");
            var builder = new StringBuilder((types.Count * 6) + 6);
            // Divider
            for(int i = 0; i <= types.Count; ++i)
            {
                builder.Append("------");
            }
            string divider = builder.ToString();
            lines.Add(divider);
            builder.Clear();

            // Types header
            builder.Append("     |");
            foreach(var type in types)
            {
                builder.Append($" {type} |");
            }
            lines.Add(builder.ToString());
            builder.Clear();

            lines.Add(divider);

            // Type chart
            foreach (var defendingType in types)
            {
                builder.Append($" {defendingType} |");
                foreach (var attackingType in types)
                {
                    var effectiveness = chart.GetEffectiveness(attackingType, defendingType);
                    if(effectiveness == TypeEffectiveness.Normal)
                    {
                        builder.Append("     |");
                    }
                    else if(effectiveness == TypeEffectiveness.NoEffect)
                    {
                        builder.Append("  0  |");
                    }
                    else if(effectiveness == TypeEffectiveness.NotVeryEffective)
                    {
                        builder.Append(" 1/2 |");
                    }
                    else if (effectiveness == TypeEffectiveness.SuperEffective)
                    {
                        builder.Append("  2  |");
                    }
                }
                lines.Add(builder.ToString());
                builder.Clear();
            }
        }

        private void GeneratePokemonInfo(RomData data, ref List<string> lines)
        {
            Header(ref lines, "Pokémon Info");
            lines.Add("Pkmn Name      |  HP  AT  DF  SP  SA  SD   |    EV Yields   | Type(s) |  Ability 1     | Ability 2     | Held Item 1   | Held Item 2   | ");
            lines.Add("-------------------------------------------------------------------------------------------------------------------------------------");
            var pkmnSorted = data.PokemonNationalDexOrder;
            foreach (var pkmn in pkmnSorted)
            {
                lines.Add(pkmn.ToString());
            }

            Header(ref lines, "Pokémon Moveset and TM/Tutor Compatibility List");
            foreach (var pkmn in pkmnSorted)
            {
                SubHeader(ref lines, NameFormatted(pkmn));
                lines.Add($"Level Up: {pkmn.learnSet}");
                lines.Add($"TM: {MoveCompatibility(pkmn.TMCompat, data.TMMoves, "TM")}");
                lines.Add($"HM: {MoveCompatibility(pkmn.HMCompat, data.HMMoves, "HM")}");
                lines.Add($"Tutor: {MoveCompatibility(pkmn.moveTutorCompat, data.TutorMoves)}");
                lines.Add($"Egg: {EnumArray(pkmn.eggMoves)}");
            }

            Header(ref lines, "Evolution Info");
            foreach (var pkmn in pkmnSorted)
            {
                if (pkmn.HasRealEvolution)
                {
                    lines.Add($"{NameFormatted(pkmn)}| {string.Join(", ", pkmn.evolvesTo.Where(e => e.IsRealEvolution))}");
                }
            }
        }

        private static string MoveCompatibility(BitArray compat, Move[] moves, string name = null)
        {
            var str = new StringBuilder();
            for (int i = 0; i < compat.Count && i < moves.Length; i++)
            {
                if (!compat[i])
                    continue;
                if (str.Length > 0)
                {
                    str.Append(", ");
                }
                if (!string.IsNullOrEmpty(name))
                {
                    str.Append($"{name}{i + 1} {moves[i].ToDisplayString()}");
                }
                else
                {
                    str.Append(moves[i].ToDisplayString());

                }
            }
            return str.Length > 0 ? str.ToString() : none;
        }

        private static string EnumArray<T>(IEnumerable<T> input) where T : System.Enum
        {
            var str = new StringBuilder();
            foreach (var i in input)
            {
                if (str.Length > 0)
                {
                    str.Append(", ");
                }
                str.Append(i.ToDisplayString());
            }
            return str.Length > 0 ? str.ToString() : none;
        }

        private static string NameFormatted(PokemonBaseStats pkmn)
        {
            return pkmn.Name + new string(' ', 15 - pkmn.Name.Length);
        }
    }
}