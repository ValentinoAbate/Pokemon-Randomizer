using PokemonRandomizer.UI.Models;

namespace PokemonRandomizer.UI.Views
{
    public class VariantPokemonDataView : DataView<VariantPokemonDataModel>
    {
        public VariantPokemonDataView(VariantPokemonDataModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Variant Pokemon");
            stack.Description("This feature creates type variants of Pokemon, and edits their movesets, color palettes, base stats, and EV yields to fit their new type!", "This feature is designed to be configured at a 10-25% chance.");
            stack.Add(new RandomChanceUI("Generate Variant Pokemon", model.CreateVariants, model.VariantChance));
        }
    }
}
