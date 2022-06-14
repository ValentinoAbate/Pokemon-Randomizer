using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.GenIII.Constants.AttributeNames;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen4RomParser : DSRomParser
    {
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            // Parse the NDS file structure
            var dsFileSystem = ParseNDSFileSystemData(rom);
            // Actually parse the ROM data
            RomData data = new RomData();
            var pokemon = ReadPokemonBaseStats(rom, dsFileSystem, info);
            foreach(var p in pokemon)
            {
                Logger.main.Info(p.ToString());
                Logger.main.Info(p.learnSet.ToString());
                if (p.HasRealEvolution)
                {
                    Logger.main.Info(string.Join(", ", p.evolvesTo.Where(e => e.IsRealEvolution)));
                }
            }

            throw new NotImplementedException("Gen IV Rom parsing not supported");
        }

        protected override Pokemon InternalIndexToPokemon(int internalIndex)
        {
            return PokemonUtils.Gen4InternalToPokemon(internalIndex);
        }

        private List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, DSFileSystemData dsFileSystem, XmlManager info)
        {
            if(!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.pokemonBaseStats), out var pokemonNARC))
            {
                return new List<PokemonBaseStats>();
            }
            var pokemon = new List<PokemonBaseStats>(pokemonNARC.FileCount);
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.movesets), out var learnsetNARC))
            {
                return new List<PokemonBaseStats>();
            }
            int evolutionsPerPokemon = info.IntAttr(ElementNames.evolutions, AttributeNames.evolutionsPerPokemon);
            int evolutionPadding = info.Padding(ElementNames.evolutions);
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.evolutions), out var evolutionsNARC))
            {
                return new List<PokemonBaseStats>();
            }
            for (int i = 1; i < pokemonNARC.FileCount; ++i)
            {
                if (!pokemonNARC.GetFile(i, out int pokemonOffset, out _, out _))
                {
                    continue;
                }
                var newPokemon = ReadBaseStatsSingle(rom, pokemonOffset, InternalIndexToPokemon(i));
                // Read Learnset
                if (learnsetNARC.GetFile(i, out int learnsetOffset, out _, out _))
                {
                    ReadLearnSet(rom, learnsetOffset, out newPokemon.learnSet);
                }
                else
                {
                    Logger.main.Error($"Unable to find learset file for pokemon {newPokemon.Name}");
                }
                // Read evolutions
                if (evolutionsNARC.GetFile(i, out int evolutionOffset, out _, out _))
                {
                    ReadEvolutions(rom, evolutionOffset, evolutionsPerPokemon, evolutionPadding, out newPokemon.evolvesTo);
                }
                else
                {
                    newPokemon.evolvesTo = Array.Empty<Evolution>();
                    Logger.main.Error($"Unable to find evolutions file for pokemon {newPokemon.Name}");
                }
                pokemon.Add(newPokemon);
            }
            return pokemon;
        }

        // TODO: refactor to reuse code from gen 3 ROM parser
        private PokemonBaseStats ReadBaseStatsSingle(Rom rom, int offset, Pokemon species)
        {
            var pkmn = new PokemonBaseStats
            {
                // Set species
                species = species
            };
            // Seek the offset of the pokemon base stats data structure
            rom.Seek(offset);
            // fill in stats (hp/at/df/sp/sa/sd)
            pkmn.stats = rom.ReadBlock(6);
            // fill in types
            pkmn.types[0] = (PokemonType)rom.ReadByte();
            pkmn.types[1] = (PokemonType)rom.ReadByte();
            pkmn.catchRate = rom.ReadByte();
            pkmn.baseExpYield = rom.ReadByte();
            // fill in ev yields (stored in the first 12 bits of data[10-11])
            pkmn.evYields = rom.ReadBits(12, 2);
            pkmn.heldItems[0] = ItemUtils.Gen4InternalToItem(rom.ReadUInt16());
            pkmn.heldItems[1] = ItemUtils.Gen4InternalToItem(rom.ReadUInt16());
            pkmn.genderRatio = rom.ReadByte();
            pkmn.eggCycles = rom.ReadByte();
            pkmn.baseFriendship = rom.ReadByte();
            pkmn.growthType = (ExpGrowthType)rom.ReadByte();
            // fill in egg groups
            pkmn.eggGroups[0] = (EggGroup)rom.ReadByte();
            pkmn.eggGroups[1] = (EggGroup)rom.ReadByte();
            // fill in abilities
            pkmn.abilities[0] = AbilityUtils.PostGen3InternalToAbility(rom.ReadByte());
            pkmn.abilities[1] = AbilityUtils.PostGen3InternalToAbility(rom.ReadByte());
            pkmn.safariZoneRunRate = rom.ReadByte();
            byte searchFlip = rom.ReadByte();
            // read color
            pkmn.searchColor = (SearchColor)((searchFlip & 0b1111_1110) >> 1);
            // read flip
            pkmn.flip = (searchFlip & 0b0000_0001) == 1;
            // Temp until these can actually be read
            pkmn.Name = species.ToDisplayString();
            pkmn.NationalDexIndex = (int)species;
            return pkmn;
        }
    }
}
