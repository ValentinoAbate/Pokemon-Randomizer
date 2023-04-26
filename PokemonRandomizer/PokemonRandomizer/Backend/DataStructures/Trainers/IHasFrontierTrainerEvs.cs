using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasFrontierTrainerEvs
    {
        [System.Flags]
        public enum EvFlags
        {
            None = 0,
            HP = 1,
            Atk = 2,
            Def = 4,
            Speed = 8,
            SpAtk = 16,
            SpDef = 32,
        }

        public EvFlags Evs { get; set; }
    }
}
