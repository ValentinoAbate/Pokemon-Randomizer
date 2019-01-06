using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class Connection
    {
        int type;
        int offset;
        byte bankId;
        byte mapId;
        int unknown;
        /// <summary> Creates a new map connection at the Rom's current internal offset</summary>
        public Connection(Rom rom)
        {
            type = rom.ReadUInt32();
            offset = rom.ReadUInt32();
            bankId = rom.ReadByte();
            mapId = rom.ReadByte();
            unknown = rom.ReadUInt16();
        }
    }
}
