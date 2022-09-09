using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;
using System;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen4RomWriter : DSRomWriter
    {
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;
        public override Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings)
        {
            var rom = new Rom(originalRom);
            // Parse the NDS file structure
            var dsFileSystem = new DSFileSystemData(rom);

            WritePokemonBaseStats(data, rom, dsFileSystem, info);
#if !DEBUG
            throw new NotImplementedException("Gen IV Rom writing not supported");
#else
            return rom;
#endif
        }
        private void WritePokemonBaseStats(RomData romData, Rom rom, DSFileSystemData dsFileSystem, XmlManager info)
        {
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.pokemonBaseStats), out var pokemonNARC))
            {
                return;
            }
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.movesets), out var learnsetNARC))
            {
                return;
            }
            int evolutionsPerPokemon = info.IntAttr(ElementNames.evolutions, AttributeNames.evolutionsPerPokemon);
            int evolutionPadding = info.Padding(ElementNames.evolutions);
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.evolutions), out var evolutionsNARC))
            {
                return;
            }
            int numTms = info.Num(ElementNames.tmMoves);
            int numHms = info.Num(ElementNames.hmMoves);
            for (int i = 1; i < romData.Pokemon.Count; ++i)
            {
                if (!pokemonNARC.GetFile(i, out int pokemonOffset, out _, out _))
                {
                    continue;
                }
                WriteBaseStatsSingle(romData.GetBaseStats(InternalIndexToPokemon(i)), pokemonOffset, rom);
                //// Read TM / HM compat
                //newPokemon.TMCompat = ReadTmHmCompat(rom, pokemonOffset, numTms, numHms, ref compatBuffer, out newPokemon.HMCompat);
                //// Read Learnset
                //if (learnsetNARC.GetFile(i, out int learnsetOffset, out _, out _))
                //{
                //    ReadLearnSet(rom, learnsetOffset, out newPokemon.learnSet);
                //}
                //else
                //{
                //    Logger.main.Error($"Unable to find learnset file for pokemon {newPokemon.Name}");
                //}
                //// Read evolutions
                //if (evolutionsNARC.GetFile(i, out int evolutionOffset, out _, out _))
                //{
                //    ReadEvolutions(rom, evolutionOffset, evolutionsPerPokemon, evolutionPadding, out newPokemon.evolvesTo);
                //}
                //else
                //{
                //    newPokemon.evolvesTo = Array.Empty<Evolution>();
                //    Logger.main.Error($"Unable to find evolutions file for pokemon {newPokemon.Name}");
                //}
            }
        }
    }
}
