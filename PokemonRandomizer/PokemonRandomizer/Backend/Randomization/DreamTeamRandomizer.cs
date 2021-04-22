using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Randomization
{
    using EnumTypes;
    using DataStructures;
    using Utilities;
    using System.Linq;

    public class DreamTeamRandomizer
    {
        private readonly IDataTranslator dataT;
        private readonly Random rand;
        public DreamTeamRandomizer(Random rand, IDataTranslator dataT)
        {
            this.dataT = dataT;
            this.rand = rand;
        }
        public Pokemon[] GenerateDreamTeam(IEnumerable<Pokemon> all, Settings.DreamTeamSettings settings)
        {
            var choices = new List<PokemonBaseStats>(all.Select(dataT.GetBaseStats));
            if (settings.BanLegendaries)
            {
                choices.RemoveAll(p => p.IsLegendary);
            }
            if (settings.BanIllegalEvolutions)
            {
                choices.RemoveAll(p => !p.IsBasicOrEvolvesFromBaby);
            }
            if (settings.UseTypeFilter)
            {
                bool TypeFilter(PokemonBaseStats p)
                {
                    if(p.IsSingleTyped)
                    {
                        return !settings.TypeFilter.Contains(p.types[0]);
                    }
                    return !settings.TypeFilter.Any(t => t == p.types[0] || t == p.types[1]);
                }
                choices.RemoveAll(TypeFilter);
            }
            var party = new List<(Pokemon pokemon, int maxBst)>(6);
            var bstDict = choices.ToDictionary(p => p.species, GetMaxBst);
            while(choices.Count > 0)
            {
                var pokemon = rand.Choice(choices);
                choices.Remove(pokemon);
                party.Add((pokemon.species, bstDict[pokemon.species]));
                if (party.Count < 6)
                    continue;
                if (!settings.UseTotalBST)
                    break;
                int bstTotal = party.Sum(t => t.maxBst);
                if (bstTotal > settings.BstTotalUpperBound)
                {
                    party.Sort((t1, t2) => t2.maxBst.CompareTo(t1.maxBst));
                    choices.RemoveAll(p => bstDict[p.species] >= party[0].maxBst);
                    if (choices.Count <= 0)
                        break;
                    party.RemoveAt(0);
                }
                else if (bstTotal < settings.BstTotalLowerBound)
                {
                    party.Sort((t1, t2) => t1.maxBst.CompareTo(t2.maxBst));
                    choices.RemoveAll(p => bstDict[p.species] <= party[0].maxBst);
                    if (choices.Count <= 0)
                        break;
                    party.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
            return party.Select(t => t.pokemon).ToArray();
        }

        private int GetMaxBst(PokemonBaseStats p)
        {
            if (!p.HasRealEvolution)
                return p.BST;
            int max = p.BST;
            foreach(var evo in p.evolvesTo)
            {
                if (!evo.IsRealEvolution)
                    continue;
                max = System.Math.Max(max, GetMaxBst(dataT.GetBaseStats(evo.Pokemon)));
            }
            return max;
        }

        public void ApplyDreamTeam(EncounterSet encounterSet, Pokemon[] team)
        {
            for(int i = 0; i < encounterSet.encounters.Count; ++i)
            {
                encounterSet.encounters[i].pokemon = team[i % team.Length];
            }
        }
    }
}
