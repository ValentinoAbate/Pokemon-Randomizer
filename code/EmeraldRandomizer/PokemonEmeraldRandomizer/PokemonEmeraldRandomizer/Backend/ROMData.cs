using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemoneEmeraldRandomizer.Backend
{
    public class ROMData
    {
        public byte[] RawROM { get; }
        public TrainerPokemon[] Starters { get; set; }
        public List<Pokemon> Pokemon { get; set; }
        public List<Trainer> Trainers { get; set; }
        public ROMData(byte[] rawROM)
        {
            RawROM = rawROM;
        }
    }
}
