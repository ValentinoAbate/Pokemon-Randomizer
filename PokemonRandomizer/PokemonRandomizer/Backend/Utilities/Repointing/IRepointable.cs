using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities.Repointing
{
    internal interface IRepointable
    {
        bool NeedsRepoint { get; }
    }
}
