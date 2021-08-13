using static PokemonRandomizer.Settings;
using ItemSettings = PokemonRandomizer.Backend.Randomization.ItemRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    using Backend.EnumTypes;
    using Utilities;
    public class ItemDataModel : DataModel
    {

        public Box<bool> DontRandomizeTms { get; } = new Box<bool>(false);

        public Box<PcItemOption> PcPotionOption { get; } = new Box<PcItemOption>(PcItemOption.Unchanged);
        public Box<Item> CustomPcItem { get; } = new Box<Item>(Item.Potion);
        public ItemSettings PcItemSettings { get; } = new ItemSettings()
        {
            SamePocketChance = 0,
        };

        public Box<bool> RandomizeFieldItems { get; } = new Box<bool>(true);
        public Box<double> FieldItemRandChance { get; } = new Box<double>(1);
        public ItemSettings FieldItemSettings { get; } = new ItemSettings()
        {
            SamePocketChance = 0.75,
        };
        public Box<bool> UseSeperateHiddenItemSettings { get; } = new Box<bool>(false);
        public Box<bool> RandomizeHiddenItems { get; } = new Box<bool>(true);
        public Box<double> HiddenItemRandChance { get; } = new Box<double>(1);
        public  ItemSettings HiddenItemSettings { get; } = new ItemSettings()
        {
            SamePocketChance = 0.75,
        };
    }
}
