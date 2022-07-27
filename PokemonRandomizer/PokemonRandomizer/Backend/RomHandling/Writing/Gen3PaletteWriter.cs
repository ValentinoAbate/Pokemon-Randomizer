using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public class Gen3PaletteWriter
    {
        public const int bytesPerColor = Backend.RomHandling.Parsing.Gen3PaletteParser.bytesPerColor;
        public void WriteCompressed(Palette palette, Rom rom)
        {
            rom.CompressToLZ77AndWriteData(ToByteArray(palette));
        }
        public void WriteCompressed(int offset, Palette palette, Rom rom)
        {
            rom.CompressToLZ77AndWriteData(offset, ToByteArray(palette));
        }

        public void WriteUncompressed(Palette palette, Rom rom)
        {
            rom.WriteBlock(ToByteArray(palette));
        }
        public void WriteUncompressed(int offset, Palette palette, Rom rom)
        {
            rom.WriteBlock(offset, ToByteArray(palette));
        }

        public byte[] ToByteArray(Palette palette)
        {
            var bytes = new byte[palette.Colors.Length * bytesPerColor];
            for (int i = 0; i < palette.Colors.Length; ++i)
            {
                var color = palette.Colors[i];
                int colorUint16 = color.r | color.g << 5 | color.b << 10 | color.a << 15;
                bytes.WriteUInt16(i * bytesPerColor, colorUint16);
            }
            return bytes;
        }
    }
}
