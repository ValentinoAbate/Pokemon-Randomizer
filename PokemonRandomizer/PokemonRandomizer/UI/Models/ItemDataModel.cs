using static PokemonRandomizer.Backend.DataStructures.ItemData;
using static PokemonRandomizer.Settings;
using ItemSettings = PokemonRandomizer.Backend.Randomization.ItemRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    using Backend.EnumTypes;
    using PokemonRandomizer.Backend.DataStructures;
    using Utilities;
    public class ItemDataModel : DataModel
    {

        public Box<bool> DontRandomizeTms { get; set; } = new Box<bool>(false);

        public Box<PcItemOption> PcPotionOption { get; set; } = new Box<PcItemOption>(PcItemOption.Unchanged);
        public Box<Item> CustomPcItem { get; set; } = new Box<Item>(Item.Potion);
        public ItemSettings PcItemSettings { get; set; } = new ItemSettings()
        {
            SamePocketChance = 0,
        };
        public Box<bool> AddItemToPokemarts { get; set; } = new Box<bool>(false);
        public Box<Item> CustomMartItem { get; set; } = new Box<Item>(Item.Rare_Candy);
        public Box<bool> OverrideCustomMartItemPrice { get; set; } = new Box<bool>(false);
        public Box<double> CustomMartItemPrice { get; set; } = new Box<double>(4800);

        public Box<bool> RandomizeFieldItems { get; set; } = new Box<bool>(false);
        public Box<double> FieldItemRandChance { get; set; } = new Box<double>(1);
        public ItemSettings FieldItemSettings { get; set; } = new ItemSettings()
        {
            SamePocketChance = 0.75,
        };
        public Box<bool> UseSeperateHiddenItemSettings { get; set; } = new Box<bool>(false);
        public Box<bool> RandomizeHiddenItems { get; set; } = new Box<bool>(true);
        public Box<double> HiddenItemRandChance { get; set; } = new Box<double>(1);
        public  ItemSettings HiddenItemSettings { get; set; } = new ItemSettings()
        {
            SamePocketChance = 0.75,
        };

        public Box<bool> RandomizePickupItems { get; set; } = new Box<bool>(false);
        public Box<double> PickupItemRandChance { get; set; } = new Box<double>(1);
        public ItemSettings PickupItemSettings { get; set; } = new ItemSettings()
        {
            SamePocketChance = 0.75,
        };

        // Items in these categories will not be randomized
        public Box<Categories> SkipCategories { get; set; } = new Box<Categories>(Categories.ContestScarf);
        // Items in these categories will not be selected from the random pool
        public Box<Categories> BannedCategories { get; set; } = new Box<Categories>(Categories.ContestScarf | Categories.Mail | Categories.MinigameBerry);
        // Items in these categories will be less likely to be chosen if they have been chosed before
        public Box<Categories> ReduceDuplicatesCategories { get; set; } = new Box<Categories>(Categories.TM | Categories.HeldItem);
        //public Box<double> OccurenceWeightMultiplier { get; set; } = new Box<double>(10);
        // Items in this categories will be more like to be replaced with an item from the same category when replaced
        public Box<Categories> KeepCategoryCategories { get; set; } = new Box<Categories>();
        public Box<double> SameCategoryChance { get; set; } = new Box<double>(0.75);
        public Box<bool> AllowBannedItemsWhenKeepingCategory { get; set; } = new Box<bool>(true);
    }
}
