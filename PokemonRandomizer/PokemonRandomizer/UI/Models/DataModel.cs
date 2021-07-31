using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI
{
    public abstract class DataModel
    {
        public const string defaultName = "Default";
        public virtual string Name => defaultName;
    }
}
