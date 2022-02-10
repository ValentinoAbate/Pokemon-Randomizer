using System;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    //using Random = Randomization.Random;
    public class EvolutionUtils
    {
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        public EvolutionUtils(Random rand, IDataTranslator dataT)
        {
            this.rand = rand;
            this.dataT = dataT;
        }

        /// <summary> return true if pokemon a evolves into or from pokemon b or IS pokemon b</summary>
        public bool RelatedToOrSelf(Pokemon a, Pokemon b)
        {
            return (a == b) || RelatedTo(a, b);
        }
        /// <summary> return true if pokemon a evolves into or from pokemon b</summary>
        private bool RelatedTo(Pokemon a, Pokemon b)
        {
            return EvolvesInto(a, b) || EvolvesFrom(a, b);
        }
        /// <summary> return true if pokemon a evolves into pokemon b</summary>
        private bool EvolvesInto(Pokemon a, Pokemon b)
        {
            var stats = dataT.GetBaseStats(a);
            var evos = stats.evolvesTo;
            foreach (var evo in evos)
            {
                if (!evo.IsRealEvolution)
                    continue;
                if (evo.Pokemon == b)
                    return true;
                if (EvolvesInto(evo.Pokemon, b))
                    return true;
            }
            return false;
        }
        /// <summary> return true if pokemon a evolves from pokemon b</summary>
        private bool EvolvesFrom(Pokemon a, Pokemon b)
        {
            var evos = dataT.GetBaseStats(a).evolvesFrom;
            foreach (var evo in evos)
            {
                if (!evo.IsRealEvolution)
                    continue;
                if (evo.Pokemon == b)
                    return true;
                if (EvolvesFrom(evo.Pokemon, b))
                    return true;
            }
            return false;
        }
        /// If the pokemon is an invalid level due to evolution state, revert to an earlier evolution
        public Pokemon CorrectImpossibleEvo(Pokemon species, int level)
        {
            var pokemon = dataT.GetBaseStats(species);
            while (!IsPokemonValidLevel(pokemon, level))
            {
                // Choose a random element from the pokemon this pokemon evolves from
                pokemon = dataT.GetBaseStats(rand.Choice(pokemon.evolvesFrom).Pokemon);
            }
            return pokemon.species;
        }
        /// <summary> returns false if the pokemon is an invalid level. 
        /// (due to not being high enough level to evolve to the current species) </summary>
        public bool IsPokemonValidLevel(PokemonBaseStats pokemon, int level)
        {
            if (pokemon.IsBasic) // basic pokemon
                return true;
            // Is there at least one valid evolution
            foreach (var evo in pokemon.evolvesFrom)
            {
                if (EquivalentLevelReq(evo, pokemon) <= level)
                    return true;
            }
            return false;
        }
        /// <summary> Returns the equivalent required level of an evolution for a give pokemon (including non-leveling evolutions if applicable) </summary>
        public int EquivalentLevelReq(Evolution evo, PokemonBaseStats pokemon)
        {
            if (evo.EvolvesByLevel)
                return evo.parameter;
            if (evo.EvolvesByFriendship && pokemon.IsBaby)
            {
                if (dataT.GetBaseStats(evo.Pokemon).HasRealEvolution)
                    return 10;
                return 18;
            }
            // For any other type Calculate level based on evolution tree
            if (pokemon.evolvesFrom.Count > 0)
            {
                return EquivalentLevelReq(pokemon.evolvesFrom[0], dataT.GetBaseStats(pokemon.evolvesFrom[0].Pokemon)) + 12;
            }
            else
            {
                int baseLevel = 32;
                // Is this pokemon a middle stage evolution?
                if (dataT.GetBaseStats(evo.Pokemon).HasRealEvolution)
                    baseLevel -= 8;
                return baseLevel;
            }
        }
        /// <summary> Return the maximum evolved form of the pokemon at the given level.
        /// returns a lower form if the pokemon is an invalid level.
        /// returns a random branch for evolution trees that branch </summary>
        public Pokemon MaxEvolution(Pokemon p, int level, bool restrictIllegalEvolutions)
        {
            var stats = dataT.GetBaseStats(p);
            // If illegal evolutions are disabled, and the pokemon is an illegal level, correct the impossible evolution
            if (restrictIllegalEvolutions && !IsPokemonValidLevel(stats, level))
                return CorrectImpossibleEvo(p, level);
            // Else evolve the pokemon until you can't anymore
            var evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, stats) <= level);
            while (evos.Count() > 0)
            {
                stats = dataT.GetBaseStats(rand.Choice(evos).Pokemon);
                evos = stats.evolvesTo.Where((evo) => evo.Type != EvolutionType.None && EquivalentLevelReq(evo, stats) <= level);
            }
            return stats.species;
        }
    }
}
