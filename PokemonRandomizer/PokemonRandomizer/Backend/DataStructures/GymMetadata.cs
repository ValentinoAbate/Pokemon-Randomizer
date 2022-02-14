using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class GymMetadata
    {
        public PokemonType[] Types { get; set; }
        public List<Trainer> Leaders { get; set; } = new List<Trainer>();
        public List<Trainer> GymTrainers { get; set; } = new List<Trainer>();
    }
}
