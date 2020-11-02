using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomMetadata
    {

        private const string gameCodeEm = "BPEE";
        private const string gameCodeLg = "BPGE";
        private const string gameCodeFr = "BPRE";
        private const string gameCodeRu = "AXVE";
        private const string gameCodeSp = "AXPE";

        private const int gbaRomNameOffset = 0xA0;
        private const int gbaRomNameSize = 12;
        private const int gbaRomCodeOffset = 0xAC;
        private const int gbaRomCodeSize = 4;
        private const int gbaRomMakerOffset = 0xB0;
        private const int gbaRomMakerSize = 2;
        private const int gbaRomVersionOffset = 0xBC;
        private const int gbaRomVersionSize = 1;


        public bool IsFireRedOrLeafGreen => Gen == Generation.III && (Code == gameCodeFr || Code == gameCodeLg);
        public bool IsRubySapphireOrEmerald => Gen == Generation.III && (Code == gameCodeEm || Code == gameCodeRu || Code == gameCodeSp);
        public bool IsEmerald => Gen == Generation.III && Code == gameCodeEm;
        public Generation Gen { get; private set; }
        public string Code { get; private set; }
        public int Version { get; private set; }
        public string Name { get; private set; }

        public RomMetadata(byte[] rawRom)
        {
            InitGeneration(rawRom);
            InitMetaData(rawRom);
        }

        // set the Rom generation (from the file size)
        private void InitGeneration(byte[] rawRom)
        {
            switch (rawRom.Length)
            {
                case 1048576:    // 1mb
                    Gen = Generation.I;
                    break;
                case 2097152:    // 2mb
                    Gen = Generation.II;
                    break;
                case 16777216:   // 16mb
                    Gen = Generation.III;
                    break;
                case 67108864:   // 64mb  (diamond and pearl)
                case 134217728:  // 128mb (heart gold, soul silver, and platinum)
                    Gen = Generation.IV;
                    break;
                case 268435456:  // 256mb (black and white)
                case 536870912:  // 512mb (black 2 and white 2)
                    Gen = Generation.V;
                    break;
                default:
                    //Add fallback based on file extension?
                    //Add manual override?
                    throw new Exception("rom file is not a valid length, unable to detect generation");
            }
        }

        // read code, name and version info from rom
        private void InitMetaData(byte[] rawRom)
        {
            switch (Gen)
            {
                case Generation.I:
                    break;
                case Generation.II:
                    break;
                case Generation.III:
                    Name = Encoding.ASCII.GetString(rawRom.ReadBlock(gbaRomNameOffset, gbaRomNameSize));
                    Code = Encoding.ASCII.GetString(rawRom.ReadBlock(gbaRomCodeOffset, gbaRomCodeSize));
                    Version = rawRom[gbaRomVersionOffset]; // Version is one byte
                    break;
                case Generation.IV:
                    break;
                case Generation.V:
                    break;
                case Generation.VI:
                    break;
                case Generation.VII:
                    break;
                default:
                    throw new Exception("Gen " + Gen.ToDisplayString() + " is not supported. unable to find metadata");
            }
        }
    }
}
