using PokemonRandomizer.Backend.Utilities;
using System.Linq;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MapManager
    {
        public List<Map> All => mapBanks.SelectMany((bank) => bank).ToList();
        private readonly Map[][] mapBanks;
        public MapManager(Rom rom, XmlManager data)
        {
            // Read data from XML file
            int bankPtrOffset = data.Offset("mapBankPointers");
            int ptrSize = data.Size("mapBankPointers");
            mapBanks = new Map[data.Num("mapBankPointers")][];
            int[] bankLengths = data.IntArrayAttr("maps", "bankLengths");
            int labelOffset = rom.ReadPointer(rom.FindFromPrefix(data.Attr("mapLabels", "ptrPrefix").Value));
            // Construct map data structures
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrOffset + (i * ptrSize));
                mapBanks[i] = LoadBank(rom, bankPtr, bankLengths[i], labelOffset);
            }
        }

        private Map[] LoadBank(Rom rom, int offset, int numMaps, int labelOffset)
        {
            Map[] maps = new Map[numMaps];
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapAddy = rom.ReadPointer(offset + (i * 4));
                maps[i] = LoadMap(rom, mapAddy, labelOffset);
            }
            return maps;
        }

        private Map LoadMap(Rom rom, int offset, int labelOffset)
        {
            rom.SaveOffset();
            rom.Seek(offset);

            #region Construct Map Qith Header Data

            var map = new Map()
            {
                mapDataOffset = rom.ReadPointer(),
                eventDataOffset = rom.ReadPointer(),
                mapScriptsOffset = rom.ReadPointer(),
                connectionOffset = rom.ReadPointer(),
                music = rom.ReadUInt16(),
                mapIndex = rom.ReadUInt16(),
                labelIndex = rom.ReadByte(),
                visibility = rom.ReadByte(),
                weather = rom.ReadByte(),
                mapType = rom.ReadByte(),
                unknown = rom.ReadByte(),
                unknown2 = rom.ReadByte(),
                showLabelOnEntry = rom.ReadByte(),
                battleField = rom.ReadByte(),
            };

            #endregion

            #region Read Non-Header Data
            // Map Nameame
            rom.Seek(rom.ReadPointer(labelOffset + map.labelIndex * 8 + 4));
                map.Name = rom.ReadVariableLengthString();

            // Connections
            if (map.connectionOffset != Rom.nullPointer)
                map.connections = new ConnectionData(rom, map.connectionOffset);
            #endregion

            rom.LoadOffset();
            return map;
        }
        /// <summary>
        /// Write the maps back to the file. Currently edits in place, and does not support exapansion of Map Bank Table.
        /// Currently only writes Map Header data.
        /// </summary>
        public void Write(Rom rom, XmlManager data)
        {
            int bankPtrOffset = data.Offset("mapBankPointers");
            int ptrSize = data.Size("mapBankPointers");
            int labelOffset = rom.ReadPointer(rom.FindFromPrefix(data.Attr("mapLabels", "ptrPrefix").Value));
            // Construct map data structures
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrOffset + (i * ptrSize));
                WriteBank(mapBanks[i], rom, bankPtr, labelOffset);
            }
        }
        /// <summary>
        /// Write the maps in this bank back to the file. Currently edits in place, and does not support exapansion of the given map table
        /// </summary>
        private void WriteBank(Map[] maps, Rom rom, int bankOffset, int labelOffset)
        {
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapOffset = rom.ReadPointer(bankOffset + (i * 4));
                WriteMap(maps[i], rom, mapOffset, labelOffset);
            }
        }
        /// <summary>
        /// Write a map to the rom. Currently only writes header data
        /// </summary>
        private void WriteMap(Map map, Rom rom, int mapOffset, int labelOffset)
        {
            #region Write Header Data
            rom.Seek(mapOffset);
            rom.WritePointer(map.mapDataOffset);
            rom.WritePointer(map.eventDataOffset);
            rom.WritePointer(map.mapScriptsOffset);
            rom.WritePointer(map.connectionOffset);
            rom.WriteUInt16(map.music);
            rom.WriteUInt16(map.mapIndex);
            rom.WriteByte(map.labelIndex);
            rom.WriteByte(map.visibility);
            rom.WriteByte(map.weather);
            rom.WriteByte(map.mapType);
            rom.WriteByte(map.unknown);
            rom.WriteByte(map.unknown2);
            rom.WriteByte(map.showLabelOnEntry);
            rom.WriteByte(map.battleField);
            #endregion

            #region Write Non-Header Data (Unimplemented
            // Write Map Name
            //rom.Seek(rom.ReadPointer(labelOffset + map.labelIndex * 8 + 4));
            //    rom.ReadVariableLengthString();

            //// Connections
            //if (connectionOffset != Rom.nullPointer)
            //    connections = new ConnectionData(rom, connectionOffset);
            #endregion
        }

        public Map GetMap(int bank, int map)
        {
            return mapBanks[bank][map];
        }
    }
}
