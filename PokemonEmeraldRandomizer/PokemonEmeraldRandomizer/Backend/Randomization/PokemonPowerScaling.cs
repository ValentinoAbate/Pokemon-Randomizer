using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class PowerScaling
    {
        public enum Options
        {
            None = 0,
            BaseStatsAggregate = 1,
            EvolutionStage = 2,
        }
        public delegate float ScoreFunc(PokemonBaseStats pkmn);
        public static Dictionary<PokemonSpecies, float> Calculate(IEnumerable<PokemonBaseStats> pokemon, Options options)
        {
            return Calculate(pokemon, GetScoringFuncs(options));
        }
        public static Dictionary<PokemonSpecies, float> Calculate(IEnumerable<PokemonBaseStats> pokemon, params ScoreFunc[] tieringFuncs)
        {
            var scores = new Dictionary<PokemonSpecies, float>();
            foreach(var p in pokemon)
            {
                float score = 0;
                foreach(var func in tieringFuncs)
                    score += func(p);
                if (tieringFuncs.Length > 0)
                    score = score / tieringFuncs.Length;
                scores.Add(p.species, score);
            }
            return scores;
        }
        private static ScoreFunc[] GetScoringFuncs(Options options)
        {
            if (options == 0)
                return new ScoreFunc[0];
            var funcs = new List<ScoreFunc>();
            if ((options & Options.BaseStatsAggregate) > 0)
                funcs.Add(BaseStatsAggregate);
            return funcs.ToArray();
        }
        private static float BaseStatsAggregate(PokemonBaseStats pkmn)
        {
            return pkmn.Hp + pkmn.Attack + pkmn.Defense + pkmn.SpAttack + pkmn.SpDefense + pkmn.Speed;
        }
    }
}
