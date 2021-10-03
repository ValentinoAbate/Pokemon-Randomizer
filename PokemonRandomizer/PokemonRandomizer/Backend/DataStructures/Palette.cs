using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class Palette
    {
        public Color[] Colors { get; }

        public Palette(ref Color[] colors)
        {
            Colors = colors;
        }

        public override string ToString()
        {
            return string.Join(", ", Colors);
        }
    }
}
