using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class MapManager
    {
        private Map[][] mapBanks;
        public MapManager(Rom rom, XmlManager data)
        {
            int bankPtrAddy = data.Offset("mapBankPointers");
            int ptrSize = data.Size("mapBankPointers");
            mapBanks = new Map[data.Num("mapBankPointers")][];
            int[] BankLengths = data.IntArrayAttr("maps", "bankLengths");
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrAddy + (i * ptrSize));
                mapBanks[i] = LoadBank(rom, data, bankPtr, BankLengths[i]);
            }   
        }

        private Map[] LoadBank(Rom rom, XmlManager data, int address, int numMaps)
        {
            Map[] maps = new Map[numMaps];
            for(int i = 0; i < maps.Length; ++i)
            {
                int mapAddy = rom.ReadPointer(address + (i * 4));
                maps[i] = new Map(rom, mapAddy );
            }
            return maps;
        }

        public Map GetMap(int bank, int map)
        {
            return mapBanks[bank][map];
        }
    }
}
