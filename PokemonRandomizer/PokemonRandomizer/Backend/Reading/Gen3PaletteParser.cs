using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen3PaletteParser
    {
        public const int bytesPerColor = 2;
        public Palette Parse(Rom rom)
        {
            return Parse(rom.ReadCompressedData());
        }

        public Palette Parse(byte[] uncompressed)
        {
            int numColors = uncompressed.Length / bytesPerColor;
            var colors = new Color[numColors];
            for (int i = 0; i < numColors; ++i)
            {
                colors[i] = ReadColor(uncompressed, i * bytesPerColor);
            }
            return new Palette(ref colors);
        }

        private const int rMask = 0b0000000000011111;
        private const int gMask = 0b0000001111100000;
        private const int bMask = 0b0111110000000000;
        private const int aMask = 0b1000000000000000;

        private Color ReadColor(byte[] data, int offset)
        {
            int color = data.ReadUInt16(offset);
            return new Color(color & rMask, (color & gMask) >> 5, (color & bMask) >> 10, (color & aMask) >> 15);
        }
    }
}
