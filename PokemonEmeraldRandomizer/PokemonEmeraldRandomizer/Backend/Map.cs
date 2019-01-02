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
        public Map(Rom rom, int ptr)
        {
            rom.Seek(ptr);
            mapDataPtr = rom.ReadPointer();
            eventDataPtr = rom.ReadPointer();
            mapScriptsPtr = rom.ReadPointer();
            connectionPtr = rom.ReadPointer();
            music = rom.ReadUInt16();
            mpInd = rom.ReadUInt16();
            labelIndex = rom.ReadByte();
            visibility = rom.ReadByte();
            weather = rom.ReadByte();
            mapType = rom.ReadByte();
            unknown = rom.ReadByte();
            unknown2 = rom.ReadByte();
            showLabelOnEntry = rom.ReadByte();
            battleField = rom.ReadByte();

        }
    }
}
