using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class Palette
    {
        public IReadOnlyList<Color> Colors => colors;
        private readonly Color[] colors;

        public Palette(ref Color[] colors)
        {
            this.colors = colors;
        }
    }
}
