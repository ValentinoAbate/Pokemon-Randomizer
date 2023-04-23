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

        }

        public EvFlags Evs { get; set; }
    }
}
