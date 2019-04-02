using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class EncounterSet
    {
        public enum Type
        {
            Grass,
            Surf,
            Fish,
            RockSmash,
            Headbutt,
        }
        public Type type;
        public List<Encounter> encounters;
        public int encounterRate;
    }
}
