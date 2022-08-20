using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public abstract class PokemonCommand : Command, IHasCommandInputType
    {
        public CommandInputType InputType { get; set; }
        public abstract EnumTypes.Pokemon Pokemon { get; set; }
        public virtual bool IsSource => false;
    }
}
