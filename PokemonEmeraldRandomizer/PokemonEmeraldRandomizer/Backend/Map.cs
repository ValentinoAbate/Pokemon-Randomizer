using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class Map
    {
        //Example from https://datacrystal.romhacking.net/wiki/Pok%C3%A9mon_3rd_Generation
        //      Data Type      |           Content          |   Example (from fire red)   |
        //---------------------|----------------------------|-----------------------------|
        //Pointer               Map data                     0x082DD4C0
        //Pointer               Event data                   0x083B4E50
        //Pointer               Map scripts                  0x0816545A
        //Pointer               Connections                  0x0835276C
        //Little endian Short   Music index                  0x012C (Pallet Town)
        //Little endian Short   Map pointer index?           0x004E
        //Byte                  Label index                  0x58 ("Pallet Town")
        //Byte                  Visibility(i.e HM Flash)     0x00
        //Byte                  Weather                      0x02
        //Byte                  Map type(City? Village? etc) 0x01
        //Little endian Short   ???                          0x0601
        //Byte                  Show label on entry          0
        //Byte                  In-battle field model id     0
        public int mapDataPtr;
        public int eventDataPtr;
        public int mapScriptsPtr;
        public int connectionPtr;
        public int music;
        public int mpInd;
        public byte labelIndex;
        public byte visibility;
        public byte weather;
        public byte mapType;
        public byte unknown;
        public byte unknown2;
        public byte showLabelOnEntry;
        public byte battleField;
        public Map(byte[] rom, int ptr)
        {
            mapDataPtr = rom.ReadPointer(ptr);
            eventDataPtr = rom.ReadPointer(ptr + 4);
            mapScriptsPtr = rom.ReadPointer(ptr + 8);
            connectionPtr = rom.ReadPointer(ptr + 12);
            music = rom.ReadUInt16(ptr + 13);
            mpInd = rom.ReadUInt16(ptr + 15);
            labelIndex = rom[ptr + 17];
            visibility = rom[ptr + 18];
            weather = rom[ptr + 19];
            mapType = rom[ptr + 20];
            unknown = rom[ptr + 21];
            unknown2 = rom[ptr + 22];
            showLabelOnEntry = rom[ptr + 23];
            battleField = rom[ptr + 24];

        }
    }
}
