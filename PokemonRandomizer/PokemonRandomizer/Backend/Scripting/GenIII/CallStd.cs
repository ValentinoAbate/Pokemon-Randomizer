using PokemonRandomizer.Backend.DataStructures;

namespace PokemonRandomizer.Backend.Scripting.GenIII
{
    // Call Std description: https://www.pokecommunity.com/showthread.php?t=201077
    // Scripting doc: https://sphericalice.com/romhacking/documents/script/#appendix-std
    public abstract class CallStd
    {
        public const byte giveItemObtain = 0x00;
        public const byte giveItemFind = 0x01;
        public const byte messageBoxNpc = 0x02;
        public const byte messageBoxSign = 0x03;
        public const byte messageBoxDefault = 0x04;
        public const byte messageBoxYesNo = 0x05;
        public const byte messageBoxAutoclose = 0x06;
        public const byte giveDecorationObtain = 0x07;
        public const byte registerMatchCallEm = 0x08;
        public const byte messageBoxGetPointsEm = 0x09;
        public const byte messageBoxPokenavEm = 0x0A;
        public const byte putItemAwayFRLG = 0x08;
        public const byte recieveItemFRLG = 0x09;
        public const byte unknown28 = 0x28;

        public static bool IsMsgBox(byte code, RomMetadata metadata)
        {
            if (code >= messageBoxNpc && code <= messageBoxAutoclose)
                return true;
            return (code is messageBoxGetPointsEm or messageBoxPokenavEm) && metadata.IsEmerald;
               
        }
    }
}
