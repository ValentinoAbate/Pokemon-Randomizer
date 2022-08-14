using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class CallIfCommand : CallCommand
    {
        public byte condition;

        public override string ToString()
        {
            return base.ToString() + " if " + condition;
        }
    }
}
