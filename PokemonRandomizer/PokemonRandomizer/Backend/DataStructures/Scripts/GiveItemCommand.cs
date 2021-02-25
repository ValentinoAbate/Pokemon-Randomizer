using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GiveItemCommand : Command
    {
        public Item item;
        public int amount;

        public override string ToString()
        {
            return "Get " + amount.ToString() + " " + item.ToDisplayString();
        }
    }
}
