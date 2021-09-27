using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class Palette
    {
        public IReadOnlyList<Color> Colors => colors;
        private readonly List<Color> colors;

        public Palette(IEnumerable<Color> colors)
        {
            this.colors = new List<Color>(colors);
        }
    }
}
