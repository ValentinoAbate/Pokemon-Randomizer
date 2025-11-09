using PokemonRandomizer.Backend.EnumTypes;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class EncounterSet : IEnumerable<Encounter>
    {
        public enum Type
        {
            None = -1,
            Grass,
            Surf,
            Fish,
            RockSmash,
            Headbutt,
            FishOldRod,
            FishGoodRod,
            FishSuperRod,
        }
        public Type type;
        public List<Encounter> encounters;
        public int encounterRate;
        public int bank;
        public int map;

        public EncounterSet(List<Encounter> encounters, Type type, int encounterRate, int bank, int map)
        {
            this.encounters = encounters;
            this.type = type;
            this.encounterRate = encounterRate;
            this.bank = bank;
            this.map = map;
        }

        public override string ToString()
        {
            return bank + ", " + map + ": " + type;
        }

        public IEnumerator<Encounter> GetEnumerator()
        {
            return encounters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return encounters.GetEnumerator();
        }
    }
}
