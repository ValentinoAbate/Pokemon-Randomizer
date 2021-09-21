using PokemonRandomizer.Backend.DataStructures;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    public class InfoFileGenerator
    {
        private const string divider = "===============================================================================================================================================";
        private const string unrandomized = "None (unrandomized)";
        public string[] GenerateInfoFile(RomData data, RomMetadata metadata)
        {
            var lines = new List<string>(1000);
            Header(ref lines, "Randomizer Info");
            lines.Add($"Randomizer Version : {MainWindow.version}");
            lines.Add($"Seed               : {(string.IsNullOrEmpty(data.Seed) ? unrandomized : data.Seed)}");
            lines.Add($"ROM                : {metadata.Name} ({metadata.Code})");
            lines.Add($"Generation         : {metadata.Gen}");

            GeneratePokemonInfo(data, ref lines);

            return lines.ToArray();

        }

        private const string indent = "   ";
        private static void Header(ref List<string> lines, string text)
        {
            lines.Add(string.Empty);
            lines.Add(divider);
            lines.Add(indent + text);
            lines.Add(divider);
            lines.Add(string.Empty);
        }

        private void GeneratePokemonInfo(RomData data, ref List<string> lines)
        {
            Header(ref lines, "Pokémon Info");
            lines.Add("Pkmn Name      |  HP  AT  DF  SP  SA  SD   |    EV Yields   | Type(s) |  Ability 1     | Ability 2     | Held Item 1   | Held Item 2   | ");
            lines.Add("-------------------------------------------------------------------------------------------------------------------------------------");
            var pkmnSorted = data.PokemonNationalDexOrder;
            foreach (PokemonBaseStats pkmn in pkmnSorted)
            {
                lines.Add(pkmn.ToString());
            }

            Header(ref lines, "Pokémon Moveset List");
            foreach (PokemonBaseStats pkmn in pkmnSorted)
            {
                lines.Add(pkmn.Name + new string(' ', 15 - pkmn.Name.Length) + "| " + pkmn.learnSet.ToString());
            }
        }
    }
}
