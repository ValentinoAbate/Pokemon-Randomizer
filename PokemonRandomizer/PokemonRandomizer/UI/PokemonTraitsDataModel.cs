using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI
{
    public class PokemonTraitsDataModel : DataModel
    {
        public double SingleTypeRandChance { get; set; }
        public double DualTypePrimaryRandChance { get; set; }
        public double DualTypeSecondaryRandChance { get; set; }
    }
}
