using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public enum CommandInputType
    {
        Normal, // Raw data
        Variable, // Data stored in the variable (int) Data
        Unknown, // Unknown
        Pointer, // Data stored at the location pointed to by the value
    }
}
