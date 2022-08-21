using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Scripting.GenIII
{
    public abstract class Flags
    {
        public abstract class Emerald
        {
            private const int system = 0x860;
            public const int enableShipSouthernIsland = system + 0x53; //0x8B3
            public const int enableShipBirthIsland    = system + 0x75; //0x8D5
            public const int enableShipFarawayIsland  = system + 0x76; //0x8D6
            public const int enableShipNavelRock      = system + 0x80; //0x8E0
        }
        public abstract class FRLG
        {
            private const int system = 0x800;
            public const int enableShipNavelRock      = system + 0x4A; //0x84A 
            public const int enableShipBirthIsland    = system + 0x4B; //0x84B
        }
        public abstract class RubySapphire
        {
            private const int system = 0x800;
            public const int hasEonTicket             = system + 0x53; //0x853
        }
    }
}
