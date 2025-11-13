using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class EncounterSet : IReadOnlyList<Encounter>
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
            // Special encounters
            DualSlotRuby,
            DualSlotSapphire,
            DualSlotEmerald,
            DualSlotFireRed,
            DualSlotLeafGreen,
            PokeRadar,
            Day,
            Night,
            Swarm,
            SoundsHoenn,
            SoundsSinnoh,
            NightFish,
        }

        public int Count => encounters.Count;

        public IEnumerable<Encounter> RealEncounters
        {
            get
            {
                foreach (var encounter in encounters)
                {
                    if (encounter.IsReal)
                    {
                        yield return encounter;
                    }
                }
            }
        }

        public Encounter this[int index] => encounters[index];

        public Type type;
        public List<Encounter> encounters;
        public int encounterRate;

        public EncounterSet(List<Encounter> encounters, Type type, int encounterRate)
        {
            this.encounters = encounters;
            this.type = type;
            this.encounterRate = encounterRate;
        }

        public override string ToString()
        {
            string ret = $"{type}: ";
            foreach (var enc in RealEncounters)
            {
                ret += $"{enc}, ";
            }
            return ret.TrimEnd(' ', ',');
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
