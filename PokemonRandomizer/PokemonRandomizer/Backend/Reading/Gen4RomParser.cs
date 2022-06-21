using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.GenIII.Constants.AttributeNames;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen4RomParser : DSRomParser
    {
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            // Parse the NDS file structure
            var dsFileSystem = new DSFileSystemData(rom);
            // Actually parse the ROM data
            RomData data = new RomData();
            // TM, HM, and MT Moves
            data.TMMoves = ReadTmMoves(rom, dsFileSystem, info, out data.HMMoves);
            data.tutorMoves = ReadMoveTutorMoves(rom, dsFileSystem, info);
            Logger.main.Info(string.Join(", ", data.tutorMoves));
            // Pokemon Base Stats
            var pokemon = ReadPokemonBaseStats(rom, dsFileSystem, info);
            foreach(var p in pokemon)
            {
                Logger.main.Info(p.ToString());
                Logger.main.Info(p.learnSet.ToString());
                Logger.main.Info($"TM: {InfoFileGenerator.MoveCompatibility(p.TMCompat, data.TMMoves, "TM")}");
                Logger.main.Info($"HM: {InfoFileGenerator.MoveCompatibility(p.HMCompat, data.HMMoves, "HM")}");
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

        protected override Item InternalIndexToItem(int internalIndex)
        {
            return ItemUtils.Gen4InternalToItem(internalIndex);
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
            int numTms = info.Num(ElementNames.tmMoves);
            int numHms = info.Num(ElementNames.hmMoves);
            for (int i = 1; i < pokemonNARC.FileCount; ++i)
            {
                if (!pokemonNARC.GetFile(i, out int pokemonOffset, out _, out _))
                {
                    continue;
                }
                var newPokemon = ReadBaseStatsSingle(rom, pokemonOffset, InternalIndexToPokemon(i));
                // Read TM / HM compat
                newPokemon.TMCompat = ReadTmHmCompat(rom, pokemonOffset, numTms, numHms, out newPokemon.HMCompat);
                // Read Learnset
                if (learnsetNARC.GetFile(i, out int learnsetOffset, out _, out _))
                {
                    ReadLearnSet(rom, learnsetOffset, out newPokemon.learnSet);
                }
                else
                {
                    Logger.main.Error($"Unable to find learnset file for pokemon {newPokemon.Name}");
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
            pkmn.heldItems[0] = InternalIndexToItem(rom.ReadUInt16());
            pkmn.heldItems[1] = InternalIndexToItem(rom.ReadUInt16());
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
        private BitArray ReadTmHmCompat(Rom rom, int pokemonOffset, int numTms, int numHms, out BitArray hmCompat)
        {
            // TmHm Compat is in the same Narc file as the rest of the base stats
            const int baseStatsTmHmCompatOffset = 28;
            rom.Seek(pokemonOffset + baseStatsTmHmCompatOffset);
            var compat = rom.ReadBits(numTms + numHms);
            var tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            for (int i = 0; i < compat.Length; ++i)
            {
                bool compatible = compat[i] == 1;
                if(i < numTms)
                {
                    tmCompat.Set(i, compatible);
                    continue;
                }
                int hmInd = i - numTms;
                if(hmInd >= 0 && hmInd < numHms)
                {
                    hmCompat.Set(hmInd, compatible);
                }
            }
            return tmCompat;
        }

        private Move[] ReadMoveTutorMoves(Rom rom, DSFileSystemData dsFileSystem, XmlManager info)
        {
            if (!info.HasElement(ElementNames.tutorMoves))
            {
                return Array.Empty<Move>();
            }
            // Initialize Move Array
            int num = info.Num(ElementNames.tutorMoves);
            var moves = new Move[num];
            int skip = Math.Max(0, info.Size(ElementNames.tutorMoves) - 2);
            // Get overlay data
            var overlayData = dsFileSystem.GetArm9OverlayData(rom, info.Overlay(ElementNames.tutorMoves), out int startOffset);
            if(overlayData.Length <= 0)
            {
                return Array.Empty<Move>();
            }
            // Go to offset
            overlayData.Seek(startOffset + info.FindOffset(ElementNames.tutorMoves, rom));
            for (int i = 0; i < num; ++i)
            {
                moves[i] = InternalIndexToMove(overlayData.ReadUInt16());
                overlayData.Skip(skip);
            }
            return moves;
        }
        private Move[] ReadTmMoves(Rom rom, DSFileSystemData dsFileSytem, XmlManager info, out Move[] hmMoves)
        {
            var arm9 = dsFileSytem.GetArm9Data(rom, out int arm9Start, out int arm9Length);
            if (!info.FindAndSeekOffset(ElementNames.tmMoves, arm9, arm9Start, arm9Start + arm9Length))
            {
                hmMoves = Array.Empty<Move>();
                return Array.Empty<Move>();
            }
            int numTms = info.Num(ElementNames.tmMoves);
            int numHms = info.Num(ElementNames.hmMoves);
            var tmMoves = new Move[numTms];
            for (int i = 0; i < numTms; ++i)
            {
                tmMoves[i] = InternalIndexToMove(arm9.ReadUInt16());
            }
            hmMoves = new Move[numHms];
            for (int i = 0; i < numHms; ++i)
            {
                hmMoves[i] = InternalIndexToMove(arm9.ReadUInt16());
            }
            return tmMoves;
        }
    }
}
