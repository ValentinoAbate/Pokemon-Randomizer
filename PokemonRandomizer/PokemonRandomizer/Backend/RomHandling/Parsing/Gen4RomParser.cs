﻿using PokemonRandomizer.Backend.Constants;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.RomHandling.IndexTranslators;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Parsing
{
    public class Gen4RomParser : DSRomParser
    {
        protected override IIndexTranslator IndexTranslator => Gen4IndexTranslator.Main;
        public override RomData Parse(Rom rom, RomMetadata metadata, XmlManager info)
        {
            // Parse the NDS file structure
            var dsFileSystem = new DSFileSystemData(rom);
            // Actually parse the ROM data
            RomData data = new RomData();
            // TM, HM, an d MT Moves
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
            else if (info.HasElementWithAttr(ElementNames.eggMoves, XmlManager.pathAttr))
            {
                if (dsFileSystem.GetFile(info.Path(ElementNames.eggMoves), out int startOffset, out _))
                {
                    ReadEggMoves(rom, startOffset + info.Offset(ElementNames.eggMoves), info, pokemon);
                }
            }
            // Move Tutor Compatibility
            ReadMoveTutorCompatibility(pokemon, rom, dsFileSystem, info, metadata);
            data.Pokemon = pokemon;
            // Trainers
            data.Trainers = ReadTrainers(rom, dsFileSystem, info, metadata, new List<string>());
            data.ItemData = ReadItemData(rom, dsFileSystem, info, metadata, data.MysteryGiftEventItems);
            var infoGen = new InfoFileGenerator();
            foreach (var line in infoGen.GenerateInfoFile(data, metadata))
            {
                Logger.main.Info(line);
            }
            for (int i = 0; i < data.Trainers.Count; i++)
            {
                Logger.main.Info($"Trainer {i}: {data.Trainers[i]}");
            }

#if DEBUG
            return data;
#else
            throw new NotImplementedException("Gen IV Rom parsing not supported");
#endif
        }

        private List<PokemonBaseStats> ReadPokemonBaseStats(Rom rom, DSFileSystemData dsFileSystem, XmlManager info)
        {
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.pokemonBaseStats), out var pokemonNARC))
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
        protected override PokemonBaseStats ReadBaseStatsSingle(Rom rom, int offset, Pokemon species)
        {
            var pkmn = base.ReadBaseStatsSingle(rom, offset, species);
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
                if (i < numTms)
                {
                    tmCompat.Set(i, compat[i] == 1);
                    continue;
                }
                int hmInd = i - numTms;
                if (hmInd >= 0 && hmInd < numHms)
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
            if (overlayData.Length <= 0)
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
                foreach (var p in pokemon)
                {
                    p.moveTutorCompat = new BitArray(0);
                }
                return;
            }
            Rom mtCompat;
            if (metadata.IsHGSS)
            {
                if (!dsFileSystem.GetFile(info.Path(ElementNames.tutorCompat), out int offset, out _))
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
                for (int j = 0; j < numTutorMoves; ++j)
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

        private List<ItemData> ReadItemData(Rom rom, DSFileSystemData dsFileSytem, XmlManager info, RomMetadata metadata, List<ItemData> mysteryGiftEventItems)
        {
            return new List<ItemData>();
        }
        private List<BasicTrainer> ReadTrainers(Rom rom, DSFileSystemData dsFileSystem, XmlManager info, RomMetadata metadata, List<string> classNames)
        {
            // Get trainer battle data
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.trainerBattles), out var trainerBattleNarc))
            {
                return new List<BasicTrainer>();
            }
            // Get trainer pokemon data
            if (!dsFileSystem.GetNarcFile(rom, info.Path(ElementNames.GenIV.trainerPokemon), out var trainerPokemonNarc))
            {
                return new List<BasicTrainer>();
            }
            bool isHGSS = metadata.IsHGSS;
            var ret = new List<BasicTrainer>(trainerBattleNarc.FileCount);
            for (int trainerInd = 0; trainerInd < trainerBattleNarc.FileCount && trainerInd < trainerPokemonNarc.FileCount; ++trainerInd)
            {
                var trainer = new BasicTrainer();
                if (!trainerBattleNarc.GetFile(trainerInd, out int trainerOffset, out _, out _))
                {
                    continue;
                }
                rom.Seek(trainerOffset);
                trainer.ClassNames = classNames;
                var dataType = (TrainerPokemon.DataType)rom.ReadByte();
                trainer.trainerClass = rom.ReadByte();
                int battleType2 = rom.ReadByte(); // what is this value? Seems to always be zero
                if(battleType2 != 0)
                {
                    Logger.main.Error($"Unknown Battle Type 2 detected in trainer {trainerInd}");
                }
                int numPokemon = rom.ReadByte();
                // Read trainer use items
                for (int itemInd = 0; itemInd < 4; ++itemInd)
                {
                    trainer.useItems[itemInd] = InternalIndexToItem(rom.ReadUInt16());
                }
                // Read AI flags
                trainer.AIFlags = new BitArray(new int[] { rom.ReadUInt32() });
                trainer.IsDoubleBattle = rom.ReadByte() == 2;

                if (!trainerPokemonNarc.GetFile(trainerInd, out int pokemonOffset, out _, out _))
                {
                    continue;
                }

                #region Read pokemon from pokemonOffset
                rom.Seek(pokemonOffset);
                trainer.PokemonData = new Trainer.TrainerPokemonData(dataType, numPokemon);
                // The pokemon data structures will be either 8 or 16 bytes depending on the dataType of the trainer
                for (int pokemonInd = 0; pokemonInd < numPokemon; ++pokemonInd)
                {
                    TrainerPokemon p;
                    IHasTrainerPokemonBallSeal ballSeal;

                    // Level, IVLevel, and other data (based on Gen)
                    if (isHGSS)
                    {
                        var hgssPokemon = new HGSSTrainerPokemon();
                        hgssPokemon.IVLevel = rom.ReadByte();
                        int overrideData = rom.ReadByte();
                        hgssPokemon.GenderOverride = (IHasTrainerPokemonGenderOverride.Type)(overrideData & 0b0000_1111);
                        hgssPokemon.AbilityOverride = (IHasTrainerPokemonAbilityOverride.Type)(overrideData >> 4);
                        hgssPokemon.level = rom.ReadUInt16();
                        hgssPokemon.species = InternalIndexToPokemon(rom.ReadUInt16());
                        p = hgssPokemon;
                        ballSeal = hgssPokemon;
                    }
                    else // DPPlt
                    {
                        var dpptPokemon = new DPPltTrainerPokemon();
                        dpptPokemon.IVLevel = rom.ReadUInt16();
                        dpptPokemon.level = rom.ReadUInt16();
                        int pokemonData = rom.ReadUInt16();
                        dpptPokemon.species = InternalIndexToPokemon(pokemonData & 0b0000_0011_1111_1111);
                        dpptPokemon.Form = pokemonData >> 10;
                        p = dpptPokemon;
                        ballSeal = dpptPokemon;
                    }

                    // Held Items and special moves
                    p.dataType = dataType;
                    if (dataType is TrainerPokemon.DataType.HeldItem or TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        p.heldItem = InternalIndexToItem(rom.ReadUInt16());
                    }
                    if (dataType is TrainerPokemon.DataType.SpecialMoves or TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    {
                        for (int moveInd = 0; moveInd < TrainerPokemon.numMoves; ++moveInd)
                            p.moves[moveInd] = InternalIndexToMove(rom.ReadUInt16());
                    }

                    // Ball Seal
                    ballSeal.BallSeal = rom.ReadUInt16(); // Add via interface?
                    trainer.PokemonData.Pokemon.Add(p);
                }
                #endregion

                ret.Add(trainer);
            }
            return ret;
        }
    }
}
