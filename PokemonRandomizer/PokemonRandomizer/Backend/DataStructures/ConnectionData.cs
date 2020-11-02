using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class ConnectionData
    {
        public int initialNumConnections;
        public int dataOffset;
        public List<Connection> connections = new List<Connection>();
    }
}
