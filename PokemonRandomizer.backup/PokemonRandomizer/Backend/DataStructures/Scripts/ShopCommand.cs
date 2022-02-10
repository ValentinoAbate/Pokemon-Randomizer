using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class ShopCommand : Command
    {
        public byte code;
        public int shopOffset;
        public readonly Shop shop = new Shop();
    }
}
