using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.DS
{
    public class DSFile
    {
        // -------------------------------------------------------
        // #0 Section - Generic Header
        // -------------------------------------------------------
        // Offset | Length   | Name                               | Description
        // 0x0 	  | 0x4 	 | Magic ID                           | Identifies the file format.
        // 0x4 	  | 0x4 	 | Constant                           | Always (0xFFFE0001)
        // 0x8 	  | 0x4 	 | Section Size                       | Size of this section, including the header.
        // 0xC 	  | 0x2 	 | Header Size                        | Size of this header. (Should always equal 0x10)
        // 0xE 	  | 0x2 	 | Number of Sections                 | The number of sub-sections in this section. 
        protected const int headerMagicIDOffset = 0x00;
        protected const int headerSectionSizeOffset = 0x08;
        protected const int headerSizeOffset = 0x0C;
        protected const int headerNumberOfSectionsOffset = 0x0E;
        protected const int headerSize = 0x10;
    }
}
