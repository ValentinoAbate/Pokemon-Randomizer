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
            // Egg Moves
            if (info.HasElementWithAttr(ElementNames.eggMoves, XmlManager.overlayAttr))
            {
                var eggMoveData = dsFileSystem.GetArm9OverlayData(rom, info.Overlay(ElementNames.eggMoves), out int startOffset, out _);
                ReadEggMoves(eggMoveData, startOffset + info.Offset(ElementNames.eggMoves), info, pokemon);
            }
            // Move Tutor Compatibility
            ReadMoveTutorCompatibility(pokemon, rom, dsFileSystem, info, metadata);
            foreach(var p in pokemon)
            {
                Logger.main.Info(p.ToString());
                Logger.main.Info(p.learnSet.ToString());
                Logger.main.Info($"TM: {InfoFileGenerator.MoveCompatibility(p.TMCompat, data.TMMoves, "TM")}");
                Logger.main.Info($"HM: {InfoFileGenerator.MoveCompatibility(p.HMCompat, data.HMMoves, "HM")}");
                Logger.main.Info($"Tutor: {InfoFileGenerator.MoveCompatibility(p.moveTutorCompat, data.tutorMoves)}");
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
            // Declare the tm / hm compat reading buffer here so it can be re-used for all pokemon
            int[] compatBuffer = new int[numTms + numHms];
            for (int i = 1; i < pokemonNARC.FileCount; ++i)
            {
                if (!pokemonNARC.GetFile(i, out int pokemonOffset, out _, out _))
                {
                    continue;
                }
                var newPokemon = ReadBaseStatsSingle(rom, pokemonOffset, InternalIndexToPokemon(i));
                // Read TM / HM compat
                newPokemon.TMCompat = ReadTmHmCompat(rom, pokemonOffset, numTms, numHms, ref compatBuffer, out newPokemon.HMCompat);
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
        private BitArray ReadTmHmCompat(Rom rom, int pokemonOffset, int numTms, int numHms, ref int[] compat, out BitArray hmCompat)
        {
            // TmHm Compat is in the same Narc file as the rest of the base stats
            const int baseStatsTmHmCompatOffset = 28;
            rom.Seek(pokemonOffset + baseStatsTmHmCompatOffset);
            rom.ReadBits(ref compat, numTms + numHms);
            var tmCompat = new BitArray(numTms);
            hmCompat = new BitArray(numHms);
            for (int i = 0; i < compat.Length; ++i)
            {
                if(i < numTms)
                {
                    tmCompat.Set(i, compat[i] == 1);
                    continue;
                }
                int hmInd = i - numTms;
                if(hmInd >= 0 && hmInd < numHms)
                {
                    hmCompat.Set(hmInd, compat[i] == 1);
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
            var overlayData = dsFileSystem.GetArm9OverlayData(rom, info.Overlay(ElementNames.tutorMoves), out int startOffset, out _);
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
        private void ReadMoveTutorCompatibility(List<PokemonBaseStats> pokemon, Rom rom, DSFileSystemData dsFileSystem, XmlManager info, RomMetadata metadata)
        {
            if (!info.HasElement(ElementNames.tutorMoves) || !info.HasElement(ElementNames.tutorCompat))
            {
                foreach(var p in pokemon)
                {
                    p.moveTutorCompat = new BitArray(0);
                }
                return;
            }
            Rom mtCompat;
            if (metadata.IsHGSS)
            {
                if(!dsFileSystem.GetFile(info.Path(ElementNames.tutorCompat), out int offset, out _))
                {
                    Logger.main.Error($"Error reading Move Tutor Compatibility. Can't find compatiblity file");
                    foreach (var p in pokemon)
                    {
                        p.moveTutorCompat = new BitArray(0);
                    }
                    return;
                }           
                mtCompat = rom;
                mtCompat.Seek(offset);
            }
            else
            {
                mtCompat = dsFileSystem.GetArm9OverlayData(rom, info.Overlay(ElementNames.tutorCompat), out int startOffset, out _);
                mtCompat.Seek(startOffset + info.Offset(ElementNames.tutorCompat));
            }
            int numTutorMoves = info.Num(ElementNames.tutorMoves);
            int[] compatBuffer = new int[numTutorMoves];
            int bytesPerCompat = info.Size(ElementNames.tutorCompat);
            for (int i = 0; i < pokemon.Count; ++i)
            {
                if (pokemon[i].species is Pokemon.POKÉMON_EGG or Pokemon.MANAPHY_EGG)
                {
                    pokemon[i].moveTutorCompat = new BitArray(0);
                    continue;
                }
                rom.ReadBits(rom.InternalOffset, ref compatBuffer, numTutorMoves);
                rom.Skip(bytesPerCompat);
                var compat = pokemon[i].moveTutorCompat = new BitArray(numTutorMoves);
                for(int j = 0; j < numTutorMoves; ++j)
                {
                    compat.Set(j, compatBuffer[j] == 1);
                }
            }
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

        // TODO: Convert to shared code
        private void ReadEggMoves(Rom rom, int offset, XmlManager info, List<PokemonBaseStats> pokemon)
        {
            rom.SaveAndSeekOffset(offset);
            int pkmnSigniture = info.HexAttr(ElementNames.eggMoves, AttributeNames.eggMovePokemonSigniture);
            var moves = new Dictionary<Pokemon, List<Move>>(pokemon.Count);
            var pkmn = Pokemon.None;
            int counter = 0;
            // Limit on loop just in case we are at the wrong place
            while (++counter < 3000)
            {
                int number = rom.ReadUInt16();
                if (number > pkmnSigniture + 1000 || number < 0)
                    break;
                if (number >= pkmnSigniture)
                {
                    pkmn = InternalIndexToPokemon(number - pkmnSigniture);
                    if (pkmn > Pokemon.None)
                        moves.Add(pkmn, new List<Move>());
                }
                else if (pkmn != Pokemon.None)
                {
                    moves[pkmn].Add(InternalIndexToMove(number));
                }
            }
            rom.LoadOffset();
            // Link Egg Moves
            foreach(var p in pokemon)
            {
                if (moves.ContainsKey(p.species))
                {
                    p.eggMoves = moves[p.species];
                }
            }
        }
    }
}
