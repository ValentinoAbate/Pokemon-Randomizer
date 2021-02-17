using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Scripting
{
    public static class Gen3ScriptCommands
    {
        public const byte end = 0x02;
        public const byte callstd = 0x09;
        public const byte copyVarIfNotZero = 0x1A;
    }
}
