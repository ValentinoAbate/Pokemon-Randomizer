using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.Scripting;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen3ScriptParser
    {
        public Script Parse(Rom rom, int offset)
        {
            rom.Seek(offset);
            var script = new Script();
            while(rom.Peek() != Gen3ScriptCommands.end)
            {
                switch (rom.ReadByte())
                {
                    case Gen3ScriptCommands.callstd:
                        break;
                    default: // Unknown command code, read all data into unknown command
                        break;
                }
            }
            return script;
        }
    }
}
