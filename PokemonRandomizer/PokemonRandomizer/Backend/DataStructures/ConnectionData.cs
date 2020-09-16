using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class ConnectionData
    {
        public int initialNumConnections;
        public int dataAddy;
        public List<Connection> connections = new List<Connection>();
        public ConnectionData(Rom rom, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            initialNumConnections = rom.ReadUInt32();
            dataAddy = rom.ReadPointer();
            rom.Seek(dataAddy);
            for (int i = 0; i < initialNumConnections; ++i)
                connections.Add(new Connection(rom));
            rom.LoadOffset();
        }
    }
}
