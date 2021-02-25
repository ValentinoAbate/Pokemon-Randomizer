using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class UnknownCommand : Command
    {
        public readonly byte[] data;
        public UnknownCommand(byte[] data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            if (data.Length <= 0)
                return "Invalid Command";
            return data[0].ToString("X2");
        }
    }
}
