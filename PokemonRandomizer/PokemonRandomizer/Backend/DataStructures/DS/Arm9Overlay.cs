using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    public class Arm9Overlay
    {
        public int ID { get; set; }
        public int FileID { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int RamAddress { get; set; }
        public int RamSize { get; set; }
        public int BssSize { get; set; }
        public int StaticStart { get; set; }
        public int StaticEnd { get; set; }
        public int CompressedSize { get; set; }
        public byte CompressionFlag { get; set; }
    }
}
