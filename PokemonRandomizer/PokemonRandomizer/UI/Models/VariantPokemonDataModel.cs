using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;

namespace PokemonRandomizer.UI.Models
{
    public class VariantPokemonDataModel : DataModel
    {
        public Box<bool> CreateVariants { get; set; } = new Box<bool>();
        public Box<double> VariantChance { get; set; } = new Box<double>(0.2);

        public WeightedSet<TypeTransformation> SingleTypeTransformationWeights { get; set; } = new WeightedSet<TypeTransformation>(AppSettings.AppSettings.SingleTypeVariantTransformationDefaultWeights);
        public WeightedSet<TypeTransformation> DualTypeTransformationWeights { get; set; } = new WeightedSet<TypeTransformation>(AppSettings.AppSettings.DualTypeVariantTransformationDefaultWeights);
    }
}
