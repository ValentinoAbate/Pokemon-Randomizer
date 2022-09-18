using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    public class VariantPaletteModifier
    {
        private static readonly Dictionary<PokemonType, TypeColorData> typeColorData = new Dictionary<PokemonType, TypeColorData>()
        {
            {PokemonType.FIR, new TypeColorData(new Color(26, 9, 9, 0)) },
            {PokemonType.GRS, new TypeColorData(new Color(13, 25, 8, 0)) },
            {PokemonType.WAT, new TypeColorData(new Color(16, 20, 28, 0)) },
            {PokemonType.ICE, new TypeColorData(new Color(21, 26, 31, 0)) },
            {PokemonType.FLY, new TypeColorData(new Color(25, 26, 27, 0)) },
            {PokemonType.ELE, new TypeColorData(new Color(31, 31, 15, 0)) { ValueOffset = 1 }},
            {PokemonType.BUG, new TypeColorData(new Color(28, 31, 14, 0)) },
            {PokemonType.GRD, new TypeColorData(new Color(27, 24, 4, 0)) { ValueOffset = -1 }},
            {PokemonType.RCK, new TypeColorData(new Color(12, 13, 4, 0)) { ValueOffset = -3 }},
            {PokemonType.STL, new TypeColorData(new Color(23, 25, 24, 0)){ ValueOffset = -2 }},
            {PokemonType.DRK, new TypeColorData(new Color(11, 10, 10, 0)) { ValueOffset = -8 } },
            {PokemonType.PSN, new TypeColorData(new Color(23, 15, 22, 0)) },
            {PokemonType.GHO, new TypeColorData(new Color(10, 6, 11, 0)) { ValueOffset = -6 }},
            {PokemonType.PSY, new TypeColorData(new Color(22, 14, 28, 0)) }, // 22, 16, 26 
            {PokemonType.FTG, new TypeColorData(new Color(20, 15, 11, 0)) },
            {PokemonType.DRG, new TypeColorData(new Color(10, 20, 26, 0)) },
            {PokemonType.NRM, new TypeColorData(new Color(21, 15, 11, 0)) } // 31, 26, 22
        };
        private static readonly int[] allIndices = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        public static int[] Range(int start, int end)
        {
            return Enumerable.Range(start, 1 + (end - start)).ToArray();
        }
        public static int[] PalRange(params int[] indices)
        {
            return indices;
        }
        public void ModifyPalette(Palette palette, PaletteData paletteData, PokemonType[] variantTypes)
        {
            if (variantTypes.Length <= 0)
                return;
            // If we don't have specific palette data or type color data to support this pokemon / type combo, return
            // Perhaps I should add a fallback for pokemon I haven't done specific work for
            if (paletteData == null)
            {
                ApplyColorChanges(palette, allIndices, typeColorData[variantTypes[0]]);
                return;
            }
            ApplyColorsFirstSecond(palette, paletteData, variantTypes);
        }

        private void ApplyColorsFirstSecond(Palette palette, PaletteData paletteData, PokemonType[] variantTypes)
        {
            var firstVariantType = variantTypes[0];
            var typeData = typeColorData[firstVariantType];
            ApplyColorChanges(palette, paletteData.PrimaryVariantColorIndices, paletteData.PrimaryVariantColorIndices2, typeData);
            if (variantTypes.Length > 1)
            {
                var secondVariantType = variantTypes[1];
                typeData = typeColorData[secondVariantType];
                ApplyColorChanges(palette, paletteData.SecondaryVariantColorIndices, paletteData.SecondaryVariantColorIndices2, typeData);
            }
        }

        private void ApplyColorChanges(Palette palette, int[] indices, int[] secondaryIndices, TypeColorData typeData)
        {
            foreach (int colorIndex in indices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.GetColor(color.Value);
            }
            foreach (int colorIndex in secondaryIndices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.HasSecondaryColors ? typeData.GetSecondaryColor(color.Value) : typeData.GetColor(color.Value);
            }
        }

        private void ApplyColorChanges(Palette palette, int[] indices, TypeColorData typeData)
        {
            foreach (int colorIndex in indices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.GetColor(color.Value);
            }
        }

        public class PaletteData
        {
            public int[] PrimaryVariantColorIndices { get; }
            public int[] PrimaryVariantColorIndices2 { get; }
            public int[] SecondaryVariantColorIndices { get; }
            public int[] SecondaryVariantColorIndices2 { get; }

            public PaletteData(int[] primaryColorIndices, int[] secondaryColorIndices = null, int[] primaryColorIndices2 = null, int[] secondaryColorIndices2 = null)
            {
                PrimaryVariantColorIndices = primaryColorIndices;
                PrimaryVariantColorIndices2 = primaryColorIndices2 ?? Array.Empty<int>();
                SecondaryVariantColorIndices = secondaryColorIndices ?? Array.Empty<int>();
                SecondaryVariantColorIndices2 = secondaryColorIndices2 ?? Array.Empty<int>();
            }

        }

        private class TypeColorData
        {
            private const int numColors = 32;
            public bool HasSecondaryColors => SecondaryColors.Length > 0;
            public int ValueOffset { get; set; } = 0;
            private Color[] Colors { get; }
            private Color[] SecondaryColors { get; }
            public TypeColorData(Color baseColor)
            {
                Colors = GenerateColorAtAllValues(baseColor);
                SecondaryColors = Array.Empty<Color>();
            }

            public TypeColorData(Color baseColor, Color secondaryColor)
            {
                Colors = GenerateColorAtAllValues(baseColor);
                SecondaryColors = GenerateColorAtAllValues(secondaryColor);
            }

            public Color GetColor(int value) => GetColor(Colors, value);
            public Color GetSecondaryColor(int value) => GetColor(Colors, value);

            private Color GetColor(Color[] colors, int value)
            {
                return colors[Math.Max(Math.Min(value + ValueOffset, colors.Length - 1), 0)];
            }

            private Color[] GenerateColorAtAllValues(Color baseColor)
            {
                int value = baseColor.Value;
                var colors = new Color[numColors];
                for (int i = 0; i < colors.Length; ++i)
                {
                    int offset = i - value;
                    colors[i] = new Color(baseColor.r + offset, baseColor.g + offset, baseColor.b + offset, baseColor.a);
                    colors[i].Clamp(0, 31);
                }
                return colors;
            }
        }
    }
}
