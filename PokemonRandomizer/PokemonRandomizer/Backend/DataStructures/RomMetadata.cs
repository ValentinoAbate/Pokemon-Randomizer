using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Text;

namespace PokemonRandomizer.Backend.DataStructures
{
    using Utilities.Debug;
    public class RomMetadata
    {
        // Gen III Codes
        private const string gameCodeEm = "BPE";
        private const string gameCodeLg = "BPG";
        private const string gameCodeFr = "BPR";
        private const string gameCodeRu = "AXV";
        private const string gameCodeSp = "AXP";
        // Gen IV Codes
        private const string gameCodePlatinum = "CPU";
        private const string gameCodeDi = "ADA";
        private const string gameCodePearl = "APA";
        private const string gameCodeHg = "IPK";
        private const string gameCodeSs = "IPG";

        private const int gbaRomNameOffset = 0xA0;
        private const int gbaRomNameSize = 12;
        private const int gbaRomCodeOffset = 0xAC;
        private const int gbaRomCodeSize = 4;
        private const int gbaRomMakerOffset = 0xB0;
        private const int gbaRomMakerSize = 2;
        private const int gbaRomVersionOffset = 0xBC;
        private const int gbaRomVersionSize = 1;

        // DS Header Information From http://dsibrew.org/wiki/DSi_Cartridge_Header
        private const int ndsRomNameOffset = 0x0;
        private const int ndsRomNameSize = 12;
        private const int ndsRomCodeOffset = 0xC;
        private const int ndsRomCodeSize = 4;
        private const int ndsRomVersionOffset = 0x1E;
        private const int ndsRomVersionSize = 1;

        public bool IsFireRedOrLeafGreen => Gen == Generation.III && (MatchCode(gameCodeFr) || MatchCode(gameCodeLg));
        public bool IsFireRed => Gen == Generation.III && MatchCode(gameCodeFr);
        public bool IsRubySapphireOrEmerald => Gen == Generation.III && (MatchCode(gameCodeEm) || MatchCode(gameCodeRu) || MatchCode(gameCodeSp));
        public bool IsRubyOrSapphire => Gen == Generation.III && (MatchCode(gameCodeRu) || MatchCode(gameCodeSp));
        public bool IsEmerald => Gen == Generation.III && MatchCode(gameCodeEm);
        public bool IsRuby => Gen == Generation.III && MatchCode(gameCodeRu);
        public bool IsSapphire => Gen == Generation.III && MatchCode(gameCodeSp);
        public bool IsPlatinum => Gen == Generation.IV && MatchCode(gameCodePlatinum);
        public bool IsHGSS => Gen == Generation.IV && (MatchCode(gameCodeHg) || MatchCode(gameCodeSs));
        public Generation Gen { get; private set; }
        public string Code { get; private set; }
        public int Version { get; private set; }
        public string Name { get; private set; }

        private bool MatchCode(string code)
        {
            return Code.Substring(0, 3) == code.Substring(0, 3);
        }

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
                    Logger.main.Error("Rom file is not a valid length, unable to detect generation");
                    return;
            }
        }

        // read code, name and version info from rom
        private void InitMetaData(byte[] rawRom)
        {
            switch (Gen)
            {
                case Generation.I:
                    goto default;
                case Generation.II:
                    goto default;
                case Generation.III:
                    Name = Encoding.ASCII.GetString(rawRom.ReadBlock(gbaRomNameOffset, gbaRomNameSize));
                    Code = Encoding.ASCII.GetString(rawRom.ReadBlock(gbaRomCodeOffset, gbaRomCodeSize));
                    Version = rawRom[gbaRomVersionOffset]; // Version is one byte
                    break;
                case Generation.IV:
                    Name = Encoding.ASCII.GetString(rawRom.ReadBlock(ndsRomNameOffset, ndsRomNameSize));
                    Code = Encoding.ASCII.GetString(rawRom.ReadBlock(ndsRomCodeOffset, ndsRomCodeSize));
                    Version = rawRom[ndsRomVersionOffset]; // Version is one byte
                    break;
                case Generation.V:
                case Generation.VI:
                case Generation.VII:
                default:
                    Logger.main.Warning("Gen " + Gen.ToDisplayString() + " is not currently supported for metadata parsing. Unable to initialize metadata");
                    return;
            }
            // Remove null terminators from name
            Name = Name.Replace("\0", string.Empty);
        }

        public override string ToString()
        {
            return $"{Name} ({Code}{Version})";
        }
    }
}
