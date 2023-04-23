using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    public class StarterRandomizer
    {
        private const int starterLevel = 5;
        private readonly PkmnRandomizer pokeRand;
        public StarterRandomizer(PkmnRandomizer pokeRand)
        {
            this.pokeRand = pokeRand;
        }
        public void Randomize(RomData data, List<Pokemon> pokemonSet, Settings settings)
        {
            if (settings.StarterSetting != Settings.StarterPokemonOption.Unchanged)
            {
                ChooseStarters(data, pokemonSet, settings);
                data.RandomizationResults.Add("Starters", data.Starters.Select(p => p.ToDisplayString()).ToList());
            }
            // Make sure all starters have attack moves
            if (settings.SafeStarterMovesets)
            {
                ApplySafeStarterLearnsets(data, settings.TypeChartRandomizationSetting);
            }
        }

        private void ChooseStarters(RomData data, List<Pokemon> pokemonSet, Settings settings) 
        {
            var starterSettings = settings.StarterPokemonSettings;
            if (settings.StarterSetting == Settings.StarterPokemonOption.Random)
            {
                for (int i = 0; i < data.Starters.Count; ++i)
                {
                    data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, starterLevel);
                }
            }
            else if (settings.StarterSetting == Settings.StarterPokemonOption.RandomTypeTriangle)
            {
                var triangle = pokeRand.RandomTypeTriangle(pokemonSet, data.Starters, data.TypeDefinitions, starterSettings, settings.StrongStarterTypeTriangle);
                if (triangle != null)
                {
                    data.Starters = triangle;
                }
                else // Fall back on completely random
                {
                    for (int i = 0; i < data.Starters.Count; ++i)
                    {
                        data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, starterLevel);
                    }
                }
            }
            else if (settings.StarterSetting == Settings.StarterPokemonOption.Custom)
            {
                int numStarters = Math.Min(data.Starters.Count, settings.CustomStarters.Length);
                for (int i = 0; i < numStarters; i++)
                {
                    Pokemon starter = settings.CustomStarters[i];
                    if (starter == Pokemon.None)
                    {
                        data.Starters[i] = pokeRand.RandomPokemon(pokemonSet, data.Starters[i], starterSettings, starterLevel);
                    }
                    else
                    {
                        data.Starters[i] = starter;
                    }
                }
            }
        }

        private void ApplySafeStarterLearnsets(RomData data, TypeChartRandomizer.Option typeChartSetting)
        {
            var moves = new Queue<Move>(TrainerPokemon.numMoves);
            foreach (var pkmn in data.Starters)
            {
                moves.Clear();
                var learnSet = data.GetBaseStats(pkmn).learnSet;               
                // Find current starting moves
                foreach(var entry in learnSet)
                {
                    if (entry.learnLvl > starterLevel)
                        break;
                    if(moves.Count >= TrainerPokemon.numMoves)
                    {
                        moves.Dequeue();
                    }
                    moves.Enqueue(entry.move);
                }
                // Check coverage
                if (HasFullCoverage(moves, data, typeChartSetting, out bool hasForesightCoverageMove, out bool hasForesight, out bool perfectWithTackle))
                {
                    continue;
                }
                // No coverage, apply fixes
                if (typeChartSetting == TypeChartRandomizer.Option.Invert || perfectWithTackle)
                {
                    // There are no type immunities in inverted type charts, just add tackle
                    learnSet.Add(Move.TACKLE, starterLevel);
                }
                else
                {
                    var lookup = learnSet.GetMovesLookup();
                    if (!hasForesight)
                    {
                        // Add odor sleuth / foresight
                        learnSet.Add(lookup.Contains(Move.ODOR_SLEUTH) ? Move.ODOR_SLEUTH : Move.FORESIGHT, starterLevel);
                    }
                    // Add tackle if a normal move is needed
                    if (!hasForesightCoverageMove)
                    {
                        var coverageMove = typeChartSetting == TypeChartRandomizer.Option.Swap ? Move.ASTONISH : Move.TACKLE;
                        learnSet.Add(coverageMove, starterLevel);
                    }
                }
                // Hoist up moves if neccessary
                int numLostMoves = -TrainerPokemon.numMoves;
                // Find number of inaccessible moves
                for (int i = 0; i < learnSet.Count; ++i)
                {
                    var entry = learnSet[i];
                    if (entry.learnLvl > starterLevel)
                        break;
                    ++numLostMoves;
                }
                int setIndex = 0;
                // Hoist inaccessible moves up to starterLevel + 1 if necessary
                for (int i = 0; i < numLostMoves; ++i)
                {
                    var entry = learnSet[setIndex];
                    // Just leave splash at lvl 1
                    if(entry.move == Move.SPLASH)
                    {
                        ++setIndex;
                        continue;
                    }
                    entry.learnLvl = starterLevel + 1;
                    // Remove the move and add it back to make sure it is at the right place
                    learnSet.RemoveAt(setIndex);
                    learnSet.Add(entry);
                }
            }
        }

        private bool HasFullCoverage(Queue<Move> moves, RomData data, TypeChartRandomizer.Option typeChartSetting, out bool hasForesightCoverageMove, out bool alreadyHasForesight, out bool perfectWithTackle)
        {
            hasForesightCoverageMove = false;
            bool isForesightCoverageMoveFirst = false;
            alreadyHasForesight = false; // TODO: scrappy exception
            perfectWithTackle = false;
            var attackingTypes = new List<PokemonType>(4);
            while(moves.Count > 0)
            {
                var move = moves.Dequeue();
                var moveData = data.GetMoveData(move);
                if (moveData.IsStatus)
                {
                    if(moveData.effect is MoveData.MoveEffect.Foresight)
                    {
                        alreadyHasForesight = true;
                    }
                    continue;
                }
                // Full coverage by this move alone
                if (data.TypeDefinitions.HasPerfectCoverage(moveData.type))
                {
                    return true;
                }
                // Note Gen I : foresight doesn't exist
                if (IsForesightCoverageMove(moveData.type, typeChartSetting))
                {
                    hasForesightCoverageMove = true;
                    isForesightCoverageMoveFirst = moves.Count >= (TrainerPokemon.numMoves - 1);
                }
                if (!attackingTypes.Contains(moveData.type))
                {
                    attackingTypes.Add(moveData.type);
                }
            }
            // If they already have a foresight coverage move and foresight, that is considered full coverage
            if (hasForesightCoverageMove && alreadyHasForesight)
                return true;
            // If there are no pokemon that are immune to all attacking types availible, that is considered full coverage  
            if (!data.Pokemon.Any(p => attackingTypes.All(t => data.TypeDefinitions.IsImmune(p, t))))
                return true;
            if (!attackingTypes.Contains(PokemonType.NRM))
            {
                if (attackingTypes.Count == 4)
                {
                    attackingTypes.RemoveAt(0);
                }
                attackingTypes.Add(PokemonType.NRM);
                perfectWithTackle = !data.Pokemon.Any(p => attackingTypes.All(t => data.TypeDefinitions.IsImmune(p, t)));
            }
            // Foresight coverage move move would get removed from starting set when foresight gets added, so it's irrelevant
            hasForesightCoverageMove &= !isForesightCoverageMoveFirst;
            return false;
        }

        private bool IsForesightCoverageMove(PokemonType type, TypeChartRandomizer.Option typeChartSetting)
        {
            if(typeChartSetting == TypeChartRandomizer.Option.None)
            {
                return type is PokemonType.NRM or PokemonType.FTG;
            }
            if(typeChartSetting == TypeChartRandomizer.Option.Swap)
            {
                return type is PokemonType.GHO;
            }
            return false;
        }
    }
}
