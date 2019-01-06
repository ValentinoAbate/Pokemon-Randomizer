using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class ConnectionData
    {
        public int initialNumConnections;
        public int dataAddy;
        public List<Connection> connections = new List<Connection>();
        public ConnectionData(Rom rom, int offset)
        {
            rom.Seek(offset);
            initialNumConnections = rom.ReadUInt32();
            dataAddy = rom.ReadPointer();
            rom.Seek(dataAddy);
            for (int i = 0; i < initialNumConnections; ++i)
                connections.Add(new Connection(rom));
        }
    }
}
