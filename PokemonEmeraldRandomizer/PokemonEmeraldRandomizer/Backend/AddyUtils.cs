using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public static class AddyUtils
    {
        #region Special Addresses
        // The address where the Type Effectiveness chart starts
        public const int typeEffectivenessAddy = 0x31ace8;
        // The address where the TM move mappings start
        public const int TMMovesAddy = 0x615b94;
        // The address where the HM move mappings start
        public const int HHMovesAddy = 0x615bf8;
        // The address where the move tutor move mappings start
        public const int moveTutorMovesAddy = 0x61500c;
        // The address where pokemon definitions start JPN: (2F0D70)
        public const int pokemonBaseStatsAddy = 0x3203e8;
        // The address where pokemon TM and HM compatiblilties start
        public const int TMCompatAddy = 0x31e8a0;
        // The address where pokemon move tutor compats starts
        public const int tutorCompatAddy = 0x61504c;
        // The address where pokemon movesets start
        public const int movesetAddy = 0x3230dc;
        // The address where pokemon evolution definitions start
        public const int evolutionAddy = 0x325344;
        // The address where the trainer definitions start
        public const int trainerAddy = 0x310030;
        // The address where the map headers start (from MEH .ini)
        public const int mapHeaderAddy = 0x84AA4;
        // NumBanks	= 0x22
        // MapBankSize	= 57,5,5,6,7,8,9,7,7,14,8,17,10,23,13,15,15,2,2,2,3,1,1,1,108,61,89,2,1,13,1,1,2,1
        // The address where the map definitions start (from MEH .ini)
        public const int mapsAddy = 0x849CC;

        #region MEH info
        //From MEH's ini file: https://github.com/shinyquagsire23/MEH (thanks shinyquagsire23)
        //MapHeaders      = 0x84AA4
        //Maps            = 0x849CC
        //WorldMap        = hoennmap.bmp
        //MapLabels = 0x123B44
        //HomeLevel       = 0x0009
        //WildPokemon     = 0xB4D48
        //MonsterNames    = 0x3185C8
        //MonsterPics     = 0x30A194
        //MonsterPals     = 0x303680
        //TrainerPics     = 0x305654
        //TrainerPals     = 0x30593C
        //SpriteBase      = 0x4975D8
        //SpriteColors	= 0x50BBC8
        //SpriteNormalSet = 0x509580
        //SpriteSmallSet  = 0x50952C
        //SpriteLargeSet  = 0x5095D4
        //NumSprites		= 244
        //TrainerPicCount = 93
        //MonsterPicCount = 440
        //MusicList       = em_songs.txt;rs_songs.txt
        //Engine = 0
        //MainTSPalCount	= 7
        //MainTSBlocks	= 0x200
        //LocalTSBlocks	= 0xFE
        //MainTSSize	= 0x280
        //LocalTSSize	= 0x140
        //MainTSHeight	= 0x140
        //LocalTSHeight	= 0xC0
        //NumBanks	= 0x22
        //MapBankSize	= 57,5,5,6,7,8,9,7,7,14,8,17,10,23,13,15,15,2,2,2,3,1,1,1,108,61,89,2,1,13,1,1,2,1
        #endregion

        #endregion

        // Converts an address' hex string to an integer
        public static Int32 hexToInt(string addy)
        {
            return Convert.ToInt32(addy, 16);
        }
    }
}
