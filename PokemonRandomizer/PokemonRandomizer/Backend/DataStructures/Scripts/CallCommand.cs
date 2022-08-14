using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class CallCommand : Command
    {
        public int offset;
        public Script script;
        public override string ToString()
        {
            return "call " + offset.ToString("X2");
        }
    }
}
