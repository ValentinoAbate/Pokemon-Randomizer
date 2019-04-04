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
        public int bank;
        public int map;
        public EncounterSet(Type type, int bank, int map, Rom rom, int offset, int num)
        {
            this.type = type;
            this.bank = bank;
            this.map = map;
            rom.Seek(offset);
            encounterRate = rom.ReadByte();
            // Idk what the next 3 bytes are
            rom.Skip(3);
            // Get the pointer to the actual data
            rom.Seek(rom.ReadPointer());
            // Read actual pokemon
            for (int i = 0; i < num; ++i)
            {
                int level = rom.ReadByte();
                int maxLevel = rom.ReadByte();
                PokemonSpecies pokemon = (PokemonSpecies)rom.ReadUInt16();
                encounters.Add(new Encounter(pokemon, level, maxLevel));
            }
        }
    }
}
