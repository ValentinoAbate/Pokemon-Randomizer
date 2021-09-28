using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Writing
{
    public class Gen3PaletteWriter
    {
        public const int bytesPerColor = Reading.Gen3PaletteParser.bytesPerColor;
        public void WriteCompressed(Palette palette, Rom rom)
        {
            rom.CompressAndWriteData(ToByteArray(palette));
        }
        public void WriteCompressed(int offset, Palette palette, Rom rom)
        {
            rom.CompressAndWriteData(offset, ToByteArray(palette));
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
            var bytes = new byte[palette.Colors.Count * bytesPerColor];
            for(int i = 0; i< palette.Colors.Count; ++i)
            {
                var color = palette.Colors[i];
                int colorUint16 = color.r | (color.g << 5) | (color.b << 10) | (color.a << 15);
                bytes.WriteUInt16(i * bytesPerColor, colorUint16);
            }
            return bytes;
        }
    }
}
