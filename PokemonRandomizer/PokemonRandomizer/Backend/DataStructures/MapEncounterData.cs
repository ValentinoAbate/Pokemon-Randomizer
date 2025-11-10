using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MapEncounterData
    {
        public string MapName { get; }
        public IEnumerable<EncounterSet> EncounterSets
        {
            get
            {
                foreach (var kvp in encounterSets)
                {
                    yield return kvp.Value;
                }
            }
        }
        public int RealEncounterCount
        {
            get
            {
                int count = 0;
                foreach(var set in EncounterSets)
                {
                    foreach (var encounter in set.RealEncounters) 
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        private readonly Dictionary<EncounterSet.Type, EncounterSet> encounterSets = new Dictionary<EncounterSet.Type, EncounterSet>();

        public MapEncounterData(string mapName) 
        { 
            MapName = string.IsNullOrEmpty(mapName) ? "???" : mapName;
        }

        public void AddEncounterSet(EncounterSet encounterSet)
        {
            encounterSets[encounterSet.type] = encounterSet;
        }


        public bool TryGetEncounterSet(EncounterSet.Type type, out EncounterSet encounterSet)
        {
            return encounterSets.TryGetValue(type, out encounterSet);
        }

        public override string ToString()
        {
            string ret = $"{MapName}: ";
            foreach(var encounterSet in EncounterSets)
            {
                ret += $"\n {encounterSet}";
            }
            return ret;
        }
    }
}
