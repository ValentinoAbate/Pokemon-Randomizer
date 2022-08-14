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
            Unknown1,         // May have something to do with entering/exiting via dive. Used in Emeralds underwater map before buried relic (24-70)
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
            public bool IsSimpleScriptType => type is Type.OnEnterMap or Type.SetMapTile or Type.OnEnterMapAndMenuClose or Type.OnEnterMapAndMenuClose2;
            public bool IsFlagValueScriptType => type is Type.ValidateAndLoad1 or Type.ValidateAndLoad2;
            public Type type;
            public int scriptOffset;
            public int scriptOffset2;
            public int flag;
            public int value;
            public Script script;
        }
    }
}
