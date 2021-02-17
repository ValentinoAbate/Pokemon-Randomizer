using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class UnknownCommand
    {
        public readonly byte[] data;
        public UnknownCommand(byte[] data)
        {
            this.data = data;
        }
    }
}
