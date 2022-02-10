using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Randomization
{
    using EnumTypes;
    using DataStructures;
    using System.Linq;
    using System.Collections;
    using System;

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
            // Type Restrictions
            if (settings.UseTypeFilter)
            {
                bool TypeFilter(PokemonBaseStats p)
                {
                    if(p.IsSingleTyped)
                    {
                        return !settings.TypeFilter.Contains(p.PrimaryType);
                    }
                    return !settings.TypeFilter.Any(t => t == p.PrimaryType || t == p.SecondaryType);
                }
                choices.RemoveAll(TypeFilter);
            }
            var party = new List<(Pokemon pokemon, int maxBst)>(6);
            var bstDict = choices.ToDictionary(p => p.species, GetMaxBst);
            // Individual BST restrictions
            if(settings.BstSetting == Settings.DreamTeamBstTotalOption.IndividualMax)
            {
                bool IndividualMaxFilter(PokemonBaseStats p)
                {
                    return bstDict[p.species] > settings.BstLimit;
                }
                choices.RemoveAll(IndividualMaxFilter);
            }
            else if(settings.BstSetting == Settings.DreamTeamBstTotalOption.IndividualMin)
            {
                bool IndividualMinFilter(PokemonBaseStats p)
                {
                    return bstDict[p.species] < settings.BstLimit;
                }
                choices.RemoveAll(IndividualMinFilter);
            }

            // Construct pool
            PriorityPool<PokemonBaseStats> pool;
            if (settings.PrioritizeVariants) 
            {
                pool = new PriorityPool<PokemonBaseStats>(choices.Where(p => p.IsVariant).ToList(), choices.Where(p => !p.IsVariant).ToList());
            }
            else
            {
                pool = new PriorityPool<PokemonBaseStats>(choices);
            }
            
            // Generate party
            while(!pool.Empty)
            {
                var pokemon = pool.GetRandom(rand);
                party.Add((pokemon.species, bstDict[pokemon.species]));
                // Continue until the party is full
                if (party.Count < 6)
                {
                    continue;
                }
                // If there is no Party BST limitation, break
                if (settings.BstSetting != Settings.DreamTeamBstTotalOption.TotalMax 
                    && settings.BstSetting != Settings.DreamTeamBstTotalOption.TotalMin)
                {
                    break;
                }
                // Party BST limitations
                int bstTotal = party.Sum(t => t.maxBst);
                if(settings.BstSetting == Settings.DreamTeamBstTotalOption.TotalMax)
                {
                    if (bstTotal > settings.BstLimit)
                    {
                        party.Sort((t1, t2) => t2.maxBst.CompareTo(t1.maxBst));
                        pool.RemoveAll(p => bstDict[p.species] >= party[0].maxBst);
                        if (pool.Empty)
                            break;
                        party.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (settings.BstSetting == Settings.DreamTeamBstTotalOption.TotalMin)
                {
                    if (bstTotal < settings.BstLimit)
                    {
                        party.Sort((t1, t2) => t1.maxBst.CompareTo(t2.maxBst));
                        pool.RemoveAll(p => bstDict[p.species] <= party[0].maxBst);
                        if (pool.Empty)
                            break;
                        party.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
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

        public class PriorityPool<T>
        {
            public bool Empty => poolList.Count <= 0;
            private readonly List<List<T>> poolList;

            public PriorityPool(params List<T>[] pools)
            {
                poolList = new List<List<T>>(pools);
                CullPoolList();
            }

            public T GetRandom(Random rand)
            {
                if(Empty)
                {
                    return default;
                }
                var pool = poolList[0];
                int index = rand.RandomInt(0, pool.Count);
                var choice = pool[index];
                pool.RemoveAt(index);
                CullPoolList();
                return choice;
            }

            public void RemoveAll(Predicate<T> pred)
            {
                if (Empty)
                {
                    return;
                }
                poolList[0].RemoveAll(pred);
                CullPoolList();
            }

            private void CullPoolList()
            {
                while(!Empty && poolList[0].Count <= 0)
                {
                    poolList.RemoveAt(0);
                }
            }
        }

    }
}
