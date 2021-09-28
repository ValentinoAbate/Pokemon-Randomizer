﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.Writing
{
    public class Gen3PaletteWriter
    {
        public const int bytesPerColor = Reading.Gen3PaletteParser.bytesPerColor;
        public void Write(Palette palette, Rom rom)
        {
            rom.CompressAndWriteData(ToByteArray(palette));
        }

        public byte[] ToByteArray(Palette palette)
        {
            var bytes = new byte[palette.Colors.Count * bytesPerColor];
            for(int i = 0; i< palette.Colors.Count; ++i)
            {
                var color = palette.Colors[i];
                int colorUint16 = color.R | (color.G << 5) | (color.B << 10) | (color.A << 15);
                bytes.WriteUInt16(i * bytesPerColor, colorUint16);
            }
            return bytes;
        }
    }
}
