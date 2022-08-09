using PokemonRandomizer.Backend.DataStructures.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MapScriptData
    {
        public enum Type
        {
            NoScripts,
            SetMapTile,
            ValidateAndLoad1, // Loads handler to 0x03000EB0 (playback)
            OnEnterMap,
            ValidateAndLoad2, // Loads handler to 0x03000F28 (playback)
            OnEnterMapAndMenuClose,
            Unknown1,
            OnEnterMapAndMenuClose2,
            Unknown2,
            Unknown3,
            Unknown4,
            Unknown5,
            Unknown6,
            Unknown7,
            Unknown8,
            Unknown9,
        }
        public readonly List<MapScript> scripts = new List<MapScript>();
        public class MapScript
        {
            public Type type;
            public int scriptOffset;
            public int scriptOffset2;
            public int flag;
            public int value;
            public Script script;
        }
    }
}
