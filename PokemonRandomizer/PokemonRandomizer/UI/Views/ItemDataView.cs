using System.Windows.Controls;
using System.Windows.Data;
using static PokemonRandomizer.Backend.DataStructures.ItemData;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;
    using PokemonRandomizer.Backend.Utilities;
    using System.Collections.Generic;
    using System.Linq;
    using static Settings;

    public class ItemDataView : DataView<ItemDataModel>
    {
        public ItemDataView(ItemDataModel model)
        {
            var tabs = new TabControl();

            tabs.Add(CreateItemRandomizerSettingsTab(model));
            tabs.Add(CreateFieldItemsTab(model));
            //tabs.Items.Add(CreateShopsTab(model));
            tabs.Add(CreateMiscTab(model));

            Content = tabs;
        }

        public CompositeCollection PCPotionStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged"},
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Custom"},
        };

        private List<BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem> GetItemCategoryDropDown() => new List<BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem>
        {
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Ball, Content="Poké Balls"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Medicine, Content="Medicine"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.StatIncrease, Content="Stat Boost Items", ToolTip="Items that permanently boost stats (EV-Boosting Items, PP-Ups, and Rare Candies)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.TM, Content="TMs"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Utility, Content="Utility Items", ToolTip="Miscellaneous utility items, (Repels, Escape Ropes, Poké Dolls, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.SellItem, Content="Sell Items", ToolTip="Items that have no purpose outside of being sold, (Nuggets, Big Mushrooms, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.ExchangeItem, Content="Exchange Items", ToolTip="Items that are exchanged for items or services (Shards, Shoal Materials, Heart Scales, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.BattleItem, Content="Battle Items", ToolTip="Items that are only used in battle, (X Items, Dire Hits, and Guard Spec.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.HeldItem, Content="Held Items", ToolTip="Items that have effects when held by a Pokémon (except Berries and Berry Juice)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.ContestScarf, Content="Contest Scarves"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.LuckyEgg, Content="Lucky Egg. Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Mail, Content="Mail"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Berry, Content="All Berries"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.MinigameBerry, Content="Minigame Berries", ToolTip="Berries that have no purpose except in minigames for making Poké Blocks, Berry Powder, Poffins, etc. (Razz Berry, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.BattleBerry, Content="Battle Berries", ToolTip="Berries that have effects when used on or held by a Pokémon (Oran Berries, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.EVBerry, Content="EV Berries", ToolTip="Berries that lower a Pokémon's EVs (Pomeg Berries, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.EvolutionItem, Content="Evolution Items", ToolTip="Items that allow a Pokémon to evolve (Evolution Stones, Metal Coats, etc.). Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Breeding, Content="Breeding Items", ToolTip="Items that have effects when held by a breeding Pokémon (Inscences, Light Ball, Everstone, etc.). Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Special, Content="Special Items", ToolTip="Special items that are normally very limited or rare (Master Ball, PP Max, and Liechi Berry)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Flute, Content="Flutes", ToolTip="Flutes (Blue Flute, etc.), except Key Items (Poké Flute, etc.)"},
        };

        private TabItem CreateItemRandomizerSettingsTab(ItemDataModel model)
        {
            var stack = CreateStack();
            stack.Header("General Randomization");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Banned Items", model.BannedCategories, GetItemCategoryDropDown, CategoryOrEquals)).ListBox);
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Skipped Items", model.SkipCategories, GetItemCategoryDropDown, CategoryOrEquals)).ListBox);
            stack.Header("Duplicate Reduction");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Reduce Duplicates", model.ReduceDuplicatesCategories, GetItemCategoryDropDown, CategoryOrEquals)).ListBox);
            stack.Header("Category Preservation");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Keep Category", model.KeepCategoryCategories, GetItemCategoryDropDown, CategoryOrEquals)).ListBox);
            stack.Add(new BoundSliderUI("Keep Category Chance", model.SameCategoryChance));
            stack.Add(new BoundCheckBoxUI(model.AllowBannedItemsWhenKeepingCategory, "Allow Banned Items When Keeping Category") { ToolTip="Allows Items to still randomized to a different Item in the same Category, even if that Item is Banned elsewhere." });
            return CreateTabItem("Item Randomization Settings", stack);
        }

        private void SetItemCategorySize(ListBox box)
        {
            box.Height = 50;
        }

        private Categories CategoryOrEquals(Categories c1, Categories c2)
        {
            return c1 |= c2;
        }

        private TabItem CreateMiscTab(ItemDataModel model)
        {
            var stack = CreateStack();
            stack.Header("PC Potion Randomization");
            var strategyCb = stack.Add(new EnumComboBoxUI<PcItemOption>("Randomization Strategy", PCPotionStrategyDropdown, model.PcPotionOption));
            strategyCb.BindVisibility(stack.Add(new ItemSettingsUI(model.PcItemSettings, false)), (int)PcItemOption.Random);
            strategyCb.BindVisibility(stack.Add(new EnumComboBoxUI<Item>("Custom PC Potion Item", EnumUtils.GetDisplayValues<Item>(), model.CustomPcItem, EnumUtils.GetValues<Item>().ToList())), (int)PcItemOption.Custom);

            stack.Header("Pickup Item Randomization");
            var pickupRand = stack.Add(new RandomChanceUI("Randomize Pickup Items", model.RandomizePickupItems, model.PickupItemRandChance));
            stack.Add(pickupRand.BindEnabled(new ItemSettingsUI(model.PickupItemSettings, false)));

            stack.Header("Custom Poké Mart Item");
            var customMartItemCb = stack.Add(new BoundCheckBoxUI(model.AddItemToPokemarts, "Add Custom Item To Poké Marts"));
            var customMartItemStack = customMartItemCb.BindVisibility(stack.Add(CreateStack()));
            customMartItemStack.Add(new EnumComboBoxUI<Item>("Item to Add", EnumUtils.GetDisplayValues<Item>(), model.CustomMartItem, EnumUtils.GetValues<Item>().ToList()));
            var modifyMartItemPriceCb = customMartItemStack.Add(new BoundCheckBoxUI(model.OverrideCustomMartItemPrice, "Override Item Price"));
            modifyMartItemPriceCb.BindVisibility(customMartItemStack.Add(new BoundSliderUI("Item Price", model.CustomMartItemPrice, false, 100, 100, 9800)));

            return CreateTabItem("Misc", stack);
        }

        private TabItem CreateShopsTab(ItemDataModel model)
        {
            var stack = CreateStack();
            return CreateTabItem("Shops", stack);
        }

        private TabItem CreateFieldItemsTab(ItemDataModel model)
        {
            var stack = CreateStack();
            stack.Header("Field And Gift Item Randomization");
            var fieldItemSettings = new ItemSettingsUI(model.FieldItemSettings, false);
            stack.Add(new RandomChanceUI("Randomize Field and Gift Items", model.RandomizeFieldItems, model.FieldItemRandChance, fieldItemSettings));
            stack.Add(fieldItemSettings);
            stack.Header("Hidden Item Randomization");
            var hiddenItemStack = CreateStack();
            var hiddenItemSettings = new ItemSettingsUI(model.HiddenItemSettings, false);
            hiddenItemStack.Add(new RandomChanceUI("Randomize Hidden Items", model.RandomizeHiddenItems, model.HiddenItemRandChance, hiddenItemSettings));
            hiddenItemStack.Add(hiddenItemSettings);
            var separateHiddenItemsCb = new BoundCheckBoxUI(model.UseSeperateHiddenItemSettings, "Use Separate Settings for Hidden Items");
            separateHiddenItemsCb.BindVisibility(hiddenItemStack);
            stack.Add(separateHiddenItemsCb, hiddenItemStack);
            return CreateTabItem("Field and Gift Items", stack);
        }
    }
}
