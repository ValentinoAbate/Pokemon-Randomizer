﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.Backend.Randomization
{
    public class BonusMoveGenerator
    {
        public const int learnLevelGenerationFailure = -1;
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        private readonly HashSet<Move> availableAddMoves;
        private readonly RomMetrics metrics;

        public BonusMoveGenerator(Random rand, RomData data, Settings settings)
        {
            this.rand = rand;
            dataT = data;
            metrics = data.Metrics;
            // Setup allowed move set
            availableAddMoves = EnumUtils.GetValues<Move>().ToHashSet();
            availableAddMoves.Remove(Move.None);
            if (settings.BanSelfdestruct)
            {
                availableAddMoves.RemoveWhere(m => data.GetMoveData(m).IsSelfdestruct);
            }
            if (settings.DisableAddingHmMoves)
            {
                foreach (var move in data.HMMoves)
                    availableAddMoves.Remove(move);
            }
        }

        public void GenerateBonusMoves(PokemonBaseStats pokemon, int numMoves, WeightedSet<AddMoveSource> addMoveSourceWeights)
        {
            var availableMoves = new List<Move>(availableAddMoves);
            availableMoves.RemoveAll(m => pokemon.learnSet.Learns(m));
            var availableEggMoves = pokemon.eggMoves.Where(m => availableMoves.Contains(m)).ToList();
            for (int i = 0; i < numMoves; ++i)
            {
                Move move = Move.None;
                switch (rand.Choice(addMoveSourceWeights))
                {
                    case AddMoveSource.Random:
                        move = rand.Choice(availableMoves);
                        break;
                    case AddMoveSource.EggMoves:
                        if (availableEggMoves.Count > 0)
                        {
                            move = rand.Choice(availableEggMoves);
                            availableEggMoves.Remove(move);
                        }
                        break;
                }
                if (move != Move.None)
                {
                    availableMoves.Remove(move);
                    int learnLevel = GenerateLearnLevel(move);
                    if(learnLevel != learnLevelGenerationFailure)
                    {
                        AddMoveToEvoTreeMoveSet(pokemon, move, learnLevel);
                    }
                }
            }
        }

        private void AddMoveToEvoTreeMoveSet(PokemonBaseStats p, Move m, int level, int creep = 0)
        {
            p.learnSet.Add(m, level);
            for (int i = 0; i < p.evolvesTo.Length; ++i)
            {
                var evo = p.evolvesTo[i];
                if (!evo.IsRealEvolution)
                    return;
                // Make this stable with the Dunsparse Plague
                if (evo.Pokemon == Pokemon.DUNSPARCE)
                    continue;
                // Don't add the move twice to pokemon that appear twice in the same evoltion tree
                // Occurs with fixed evolutions where the original trade evolution is left
                bool duplicate = false;
                for (int j = 0; j < i; ++j)
                {
                    if (p.evolvesTo[j].Pokemon == evo.Pokemon)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (duplicate)
                {
                    continue;
                }
                AddMoveToEvoTreeMoveSet(dataT.GetBaseStats(evo.Pokemon), m, level + creep, creep);
            }
        }

        public int GenerateLearnLevel(Move move)
        {
            if (metrics.LearnLevels.ContainsKey(move))
            {
                double mean = metrics.LearnLevelMeans[move];
                double stdDev = metrics.LearnLevelStandardDeviations[move];
                return rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
            }
            int effectivePower = dataT.GetMoveData(move).EffectivePower;
            if (metrics.LearnLevelPowers.ContainsKey(effectivePower))
            {
                double mean = metrics.LearnLevelPowerMeans[effectivePower];
                double stdDev = metrics.LearnLevelPowerStandardDeviations[effectivePower];
                return rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
            }
            if (metrics.LearnLevelPowers.Count > 0)
            {
                var powers = metrics.LearnLevelPowers.Keys.ToList();
                powers.Sort();
                int closestPower = 0;
                int last = 0;
                foreach (var power in powers)
                {
                    if (power > effectivePower)
                    {
                        closestPower = effectivePower - last > power - effectivePower ? power : last;
                        break;
                    }
                    last = power;
                }
                double mean = metrics.LearnLevelPowerMeans[closestPower];
                double stdDev = metrics.LearnLevelPowerStandardDeviations[closestPower];
                return rand.RandomGaussianPositiveNonZeroInt(mean, stdDev);
            }
            Logger.main.Error($"No metric data, can't generate a learn level for move. Returning {learnLevelGenerationFailure}");
            return learnLevelGenerationFailure;
        }
    }
}
