using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class ConnectionData
    {
        public int initialNumConnections;
        public int dataOffset;
        public List<Connection> connections = new List<Connection>();
        public ConnectionData(Rom rom, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            initialNumConnections = rom.ReadUInt32();
            dataOffset = rom.ReadPointer();
            rom.Seek(dataOffset);
            for (int i = 0; i < initialNumConnections; ++i)
                connections.Add(new Connection(rom));
            rom.LoadOffset();
        }
    }
}
