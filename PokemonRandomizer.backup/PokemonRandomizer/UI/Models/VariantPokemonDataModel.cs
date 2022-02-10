using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;

namespace PokemonRandomizer.UI.Models
{
    public class VariantPokemonDataModel : DataModel
    {
        public enum TypeTransformationWeightOption
        {
            Random,
            Balanced,
            Custom
        }


        public Box<bool> CreateVariants { get; set; } = new Box<bool>();
        public Box<double> VariantChance { get; set; } = new Box<double>(0.2);

        public Box<TypeTransformationWeightOption> TypeTransformationOption { get; set; } = new Box<TypeTransformationWeightOption>(TypeTransformationWeightOption.Balanced);
        public WeightedSet<TypeTransformation> SingleTypeTransformationWeights { get; set; } = new WeightedSet<TypeTransformation>(AppSettings.AppSettings.SingleTypeVariantTransformationDefaultWeights);
        public WeightedSet<TypeTransformation> DualTypeTransformationWeights { get; set; } = new WeightedSet<TypeTransformation>(AppSettings.AppSettings.DualTypeVariantTransformationDefaultWeights);

        public Box<bool> AdjustAttackStats { get; set; } = new Box<bool>(true);
        public Box<bool> GiveBonusStats { get; set; } = new Box<bool>(true);
        public Box<double> BonusStatAmountMean { get; set; } = new Box<double>(15);
        public Box<double> BonusStatAmountStdDev { get; set; } = new Box<double>(5);
    }
}
