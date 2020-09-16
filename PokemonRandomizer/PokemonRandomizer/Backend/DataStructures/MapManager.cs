using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MapManager
    {
        private Map[][] mapBanks;
        public MapManager(Rom rom, XmlManager data)
        {
            // Read data from XML file
            int bankPtrAddy = data.Offset("mapBankPointers");
            int ptrSize = data.Size("mapBankPointers");
            mapBanks = new Map[data.Num("mapBankPointers")][];
            int[] bankLengths = data.IntArrayAttr("maps", "bankLengths");
            int labelAddress = rom.ReadPointer(rom.FindFromPrefix(data.Attr("mapLabels", "ptrPrefix").Value));
            // Construct map data structures
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrAddy + (i * ptrSize));
                mapBanks[i] = LoadBank(rom, bankPtr, bankLengths[i], labelAddress);
            }
        }

        private Map[] LoadBank(Rom rom, int address, int numMaps, int labelAddress)
        {
            Map[] maps = new Map[numMaps];
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapAddy = rom.ReadPointer(address + (i * 4));
                maps[i] = new Map(rom, mapAddy, labelAddress);
            }
            return maps;
        }

        public Map GetMap(int bank, int map)
        {
            return mapBanks[bank][map];
        }
    }
}
