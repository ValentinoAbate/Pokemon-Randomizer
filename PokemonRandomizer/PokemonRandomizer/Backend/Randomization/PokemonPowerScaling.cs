using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Randomization
{
    public static class PowerScaling
    {
        [Flags]
        public enum Options
        {
            None = 0,
            BaseStatsAggregate = 1,
            EvolutionStage = 2,
        }
        public delegate float ScoreFunc(PokemonBaseStats pkmn);
        public static Dictionary<Pokemon, float> Calculate(IEnumerable<PokemonBaseStats> pokemon, Options options)
        {
            return Calculate(pokemon, GetScoringFuncs(options));
        }
        public static Dictionary<Pokemon, float> Calculate(IEnumerable<PokemonBaseStats> pokemon, params ScoreFunc[] tieringFuncs)
        {
            var scores = new Dictionary<Pokemon, float>();
            foreach(var p in pokemon)
            {
                float score = 0;
                foreach(var func in tieringFuncs)
                    score += func(p);
                if (tieringFuncs.Length > 1) // Don't need to divide by 1
                    score /= tieringFuncs.Length;
                if(!scores.ContainsKey(p.species))
                    scores.Add(p.species, score);
            }
            return scores;
        }
        private static ScoreFunc[] GetScoringFuncs(Options options)
        {
            if (options == Options.None)
                return new ScoreFunc[0];
            var funcs = new List<ScoreFunc>();
            if (options.HasFlag(Options.BaseStatsAggregate))
                funcs.Add(BaseStatsAggregate);
            return funcs.ToArray();
        }
        private static float BaseStatsAggregate(PokemonBaseStats pkmn)
        {
            return pkmn.Hp + pkmn.Attack + pkmn.Defense + pkmn.SpAttack + pkmn.SpDefense + pkmn.Speed;
        }
    }
}
