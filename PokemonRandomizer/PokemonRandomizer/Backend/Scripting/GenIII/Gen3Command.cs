using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Scripting.GenIII
{
    public class Gen3Command
    {
        public static Dictionary<byte, Argument.Type[]> commandMap = new Dictionary<byte, Argument.Type[]>()
        {
        };
        public byte code;
        public List<Argument> args;
        public class Argument 
        { 
            public enum Type
            {
                Byte,
                Word,
                Pointer,
            }
        }
    }
}
