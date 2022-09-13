using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
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
        public void Randomize(RomData data, IEnumerable<Pokemon> pokemonSet, Settings settings)
        {
            if (settings.StarterSetting == Settings.StarterPokemonOption.Unchanged)
            {
                return;
            }
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
            // data.Starters = data.Pokemon.Where(d => d.IsBasicOrEvolvesFromBaby).Select(d => d.species).ToList();
            // Make sure all starters have attack moves
            if (settings.SafeStarterMovesets)
            {
                ApplySafeStarterLearnsets(data);
            }
        }

        private void ApplySafeStarterLearnsets(RomData data)
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
                if (HasFullCoverage(moves, data, out bool hasFightingOrNormalMove))
                {
                    continue;
                }
                // No coverage, apply fixes
                var lookup = learnSet.GetMovesLookup();
                // Add odor sleuth / foresight
                learnSet.Add(lookup.Contains(Move.ODOR_SLEUTH) ? Move.ODOR_SLEUTH : Move.FORESIGHT, starterLevel);
                // Add tackle if a normal move is needed
                if (!hasFightingOrNormalMove)
                {
                    learnSet.Add(Move.TACKLE, starterLevel);
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

        private bool HasFullCoverage(Queue<Move> moves, IDataTranslator dataT, out bool hasFightingOrNormalMove)
        {
            hasFightingOrNormalMove = false;
            bool isFightingOrNormalMoveFirst = false;
            bool alreadyHasForesight = false;
            while(moves.Count > 0)
            {
                var move = moves.Dequeue();
                var moveData = dataT.GetMoveData(move);
                if (moveData.IsStatus)
                {
                    if(moveData.effect is MoveData.MoveEffect.Foresight)
                    {
                        alreadyHasForesight = true;
                    }
                    continue;
                }
                // GHO -> NRM is immune
                // PSY -> DRK is immune
                // PSN -> STL is immune
                // GRD -> FLY is immune (+ Levitate)
                // ELE -> GRD is immune (+ Volt Absorb)
                // FIR -> Flash Fire
                // WAT -> Water Absorb + Dry Skin
                // Note Gen VI : DRG will have an immunity due to fairy existing
                // Note Gen II Note Gen I: FIR and WAT have no immunities because abilities don't exist
                // Note Gen I : PSN and PSY have no immunities because dark and steel don't exist
                if (moveData.type is PokemonType.GHO or PokemonType.PSY or PokemonType.PSN or PokemonType.GRD or PokemonType.ELE or PokemonType.FIR or PokemonType.WAT)
                {
                    continue;
                }
                // Note Gen I : foresight doesn't exist
                if(moveData.type is PokemonType.NRM or PokemonType.FTG)
                {
                    hasFightingOrNormalMove = true;
                    isFightingOrNormalMoveFirst = moves.Count >= (TrainerPokemon.numMoves - 1);
                    continue;
                }
                return true;
            }
            // If they already have a fighting or normal move and foresight, that is considered full immunity
            if (hasFightingOrNormalMove && alreadyHasForesight)
                return true;
            // Fighting / Normal move would get removed from starting set when foresight gets added, so it's irrelevant
            hasFightingOrNormalMove &= !isFightingOrNormalMoveFirst;
            return false;
        }
    }
}
