using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class SetBerryTreeCommand : Command
    {
        public byte treeId;
        public Item berry;
        public byte growthStage; // 1 (seed) - 5 (fully grown)

        public override string ToString()
        {
            return $"Tree {treeId:x2}: {berry.ToDisplayString()}";
        }
    }
}
