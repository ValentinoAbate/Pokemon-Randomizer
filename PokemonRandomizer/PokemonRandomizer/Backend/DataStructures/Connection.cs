using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class Connection
    {
        public enum Type
        {
            Down,
            Up,
            Left,
            Right,
            Dive,
            Emerge,
        }
        public Type type;
        public int offset;
        public byte bankId;
        public byte mapId;
        public int unknown;
        /// <summary> Creates a new map connection at the Rom's current internal offset</summary>
        public Connection(Rom rom)
        {
            type = (Type)rom.ReadUInt32();
            offset = rom.ReadUInt32();
            bankId = rom.ReadByte();
            mapId = rom.ReadByte();
            unknown = rom.ReadUInt16();
        }
    }
}
