using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.DS;
using PokemonRandomizer.Search;

namespace PokemonRandomizer.Backend.RomHandling.Parsing
{
    public abstract class DSRomParser : RomParser
    {
        // Search for a specific byte sequence in all overlays
        protected static bool TryFindSequenceInOverlays(byte[] sequence, Rom rom, DSFileSystemData dsFileSystem, out int overlayId, out int offset)
        {
            for (int i = 0; i < dsFileSystem.Arm9Overlays.Count; ++i)
            {
                var overlay = dsFileSystem.GetArm9OverlayData(rom, i, out int startOffset, out int length);
                int sequenceOffset = Kmp.Search(overlay.File, sequence, startOffset, startOffset + length);
                if (sequenceOffset != -1)
                {
                    overlayId = i;
                    offset = sequenceOffset - startOffset;
                    return true;
                }
            }
            overlayId = offset = -1;
            return false;
        }
    }
}
