using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class SetBerryTreeCommand : Command
    {
        public byte treeId;
        public Item berry;
        public byte unknown; // I believe this might be growth stage, but I need to test that

        public override string ToString()
        {
            return $"Tree {treeId:x2}: {berry.ToDisplayString()}";
        }
    }
}
