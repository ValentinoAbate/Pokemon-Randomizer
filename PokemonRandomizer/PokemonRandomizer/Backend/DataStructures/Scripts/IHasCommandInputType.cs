using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public interface IHasCommandInputType
    {
        public CommandInputType InputType { get; set; }
    }
}
