using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MapData
    {
        public int width; 
        public int height;
        public int borderTileOffset;
        public int mapTilesOffset;
        public int globalTileSetOffset;
        public int localTileSetOffset;
        public int borderWidth;
        public int borderHeight;
        public int secondarySize;
    }
}
