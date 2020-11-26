using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Scripting
{
    public static class Gen3Opcodes
    {
        public const byte setRegister = 0x20;
        public const byte addRegister = 0x30;
        public const byte reg0 = 0x00, reg1 = 0x01, reg2 = 0x02, reg3 = 0x03, reg4 = 0x04, reg5 = 0x05, reg6 = 0x06, reg7 = 0x07;
    }
}
