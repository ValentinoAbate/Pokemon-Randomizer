using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.Utilities;
using static PokemonRandomizer.Backend.DataStructures.ItemData;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using static Settings;

    public class ItemDataView : DataView<ItemDataModel>
    {
        public ItemDataView(ItemDataModel model, List<Item> allItems, RomMetadata metadata)
        {
            var customItemOptions = metadata.IsFireRedOrLeafGreen ? allItems.Where(i => !ItemUtils.IsTM(i) && !ItemUtils.IsHM(i)).ToList() : allItems;
            var allItemsDisplay = allItems.Select(EnumUtils.ToDisplayString).ToList();
            var tabs = new TabControl();
  
            tabs.Add(CreateFieldItemsTab(model));
            //tabs.Items.Add(CreateShopsTab(model));
            tabs.Add(CreateMiscTab(model, allItemsDisplay, allItems, metadata));
            tabs.Add(CreateItemRandomizerSettingsTab(model));

            Content = tabs;
        }

        private static CompositeCollection PCPotionStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged"},
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Custom"},
        };

        private List<BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem> GetItemCategoryDropDown() => new()
        {
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Ball, Content="Poké Balls", ToolTip="All Poké Balls"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Medicine, Content="Medicine", ToolTip="Medicine (HP, PP, and Status Healing Items, Revives, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.StatIncrease, Content="Stat Boost Items", ToolTip="Items that permanently boost stats (EV-Boosting Items, PP-Ups, and Rare Candies)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.TM, Content="TMs", ToolTip="All TMs"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Utility, Content="Utility Items", ToolTip="Miscellaneous utility items (Repels, Escape Ropes, Poké Dolls, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.SellItem, Content="Sell Items", ToolTip="Items that have no purpose outside of being sold (Nuggets, Big Mushrooms, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.ExchangeItem, Content="Exchange Items", ToolTip="Items that are exchanged for other items or services (Shards, Shoal Materials, Heart Scales, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.BattleItem, Content="Battle Items", ToolTip="Items that are only used in battle (X Items, Dire Hits, and Guard Spec.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.HeldItem, Content="Held Items", ToolTip="Items that have effects when held by a Pokémon (except Berries and Berry Juice)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.ContestScarf, Content="Contest Scarves", ToolTip="All Contest Scarves (does not include the Silk Scarf)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.LuckyEgg, Content="Lucky Egg", ToolTip="Lucky Egg. Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Mail, Content="Mail", ToolTip="All Mail"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Berry, Content="All Berries", ToolTip="All Berries. Selecting this category is the same as selecting the Minigame, Battle, and EV Berry categories separately"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.MinigameBerry, Content="Minigame Berries", ToolTip="Berries that have no purpose except in minigames for making Poké Blocks, Berry Powder, Poffins, etc. (Razz Berry, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.BattleBerry, Content="Battle Berries", ToolTip="Berries that have effects when used on or held by a Pokémon (Oran Berries, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.EVBerry, Content="EV Berries", ToolTip="Berries that lower a Pokémon's EVs (Pomeg Berries, etc.)"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.EvolutionItem, Content="Evolution Items", ToolTip="Items that allow a Pokémon to evolve (Evolution Stones, Metal Coats, etc.). Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Breeding, Content="Breeding Items", ToolTip="Items that have effects when held by a breeding Pokémon (Inscences, Light Ball, Everstone, etc.). Overlaps with Held Items"},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Special, Content="Special Items", ToolTip="Special items that are normally very limited or rare (Master Ball, PP Max, and Liechi Berry) and mystery gift event items if \"Enable Mystery Gift Events\" is on with \"Allow in Item Randomization\""},
            new BoundFlagsEnumListBoxUI<Categories>.MenuBoxItem { Item = Categories.Flute, Content="Flutes", ToolTip="Flutes (Blue Flute, etc.), except Key Items (Poké Flute, etc.)"},
        };

        private static CompositeCollection DuplicateReductionOptionDropdown => new()
        {
            new ComboBoxItem() { Content = "Weak", ToolTip = "Duplicate-reduced items will usually appear 2-3 times" },
            new ComboBoxItem() { Content = "Moderate", ToolTip = "Duplicate-reduced items will usually appear ~2 times" },
            new ComboBoxItem() { Content = "Strong", ToolTip = "Duplicate-reduced items will usually appear 1 time" },
        };

        private const string banItemTooltip = "Items in banned categories will not be chosen as a random item. Skipped or otherwise unrandomized items may still be items from a banned category";
        private const string skipItemTooltip = "Items in skipped categories will not be randomized and left as they are in the base game";
        private const string reduceDuplicatesTooltip = "Items in the marked categories will be less likely to appear multiple times as random items. Skipped or otherwise unrandomized items will count towards duplicate reduction";
        private const string keepSameCategoryTooltip = "Items in the marked categories will only randomize to an item in the same category a certain percentage of the time. For example, if TMs are marked, all TMs can only randomize to other TMs";

        private TabItem CreateItemRandomizerSettingsTab(ItemDataModel model)
        {
            var stack = CreateStack();
            stack.Header("General Randomization");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Banned Items", model.BannedCategories, GetItemCategoryDropDown, CategoryOrEquals, banItemTooltip)).ListBox);
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Skipped Items", model.SkipCategories, GetItemCategoryDropDown, CategoryOrEquals, skipItemTooltip)).ListBox);
            stack.Header("Duplicate Reduction");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Reduce Duplicates", model.ReduceDuplicatesCategories, GetItemCategoryDropDown, CategoryOrEquals, reduceDuplicatesTooltip)).ListBox);
            stack.Add(new EnumComboBoxUI<ItemDataModel.DuplicateReductionOption>("Duplicate Reduction Strength", DuplicateReductionOptionDropdown, model.DupeReductionStrength));
            stack.Header("Category Preservation");
            SetItemCategorySize(stack.Add(new BoundFlagsEnumListBoxUI<Categories>("Keep Category", model.KeepCategoryCategories, GetItemCategoryDropDown, CategoryOrEquals, keepSameCategoryTooltip)).ListBox);
            stack.Add(new BoundSliderUI("Keep Category Chance", model.SameCategoryChance));
            stack.Add(new BoundCheckBoxUI("Allow Banned Items When Keeping Category", model.AllowBannedItemsWhenKeepingCategory) { ToolTip="Allows Items in \"Keep Category\" categories to randomize to all Items in their own category, including Items in banned categories" });
            return CreateTabItem("Item Category Settings", stack);
        }

        private void SetItemCategorySize(ListBox box)
        {
            box.Height = 50;
        }

        private Categories CategoryOrEquals(Categories c1, Categories c2)
        {
            return c1 |= c2;
        }

        private TabItem CreateMiscTab(ItemDataModel model, List<string> allItemsDisplay, List<Item> allItems, RomMetadata metadata)
        {
            var stack = CreateStack();
            stack.Header("PC Potion Randomization");
            var strategyCb = stack.Add(new EnumComboBoxUI<PcItemOption>("Randomization Strategy", PCPotionStrategyDropdown, model.PcPotionOption));
            strategyCb.BindVisibility(stack.Add(new ItemSettingsUI(model.PcItemSettings, false)), (int)PcItemOption.Random);
            strategyCb.BindVisibility(stack.Add(new EnumComboBoxUI<Item>("Custom PC Potion Item", allItemsDisplay, model.CustomPcItem, allItems)), (int)PcItemOption.Custom);

            stack.Header("Pickup Item Randomization");
            var pickupRand = stack.Add(new RandomChanceUI("Randomize Pickup Items", model.RandomizePickupItems, model.PickupItemRandChance));
            stack.Add(pickupRand.BindEnabled(new ItemSettingsUI(model.PickupItemSettings, false)));

            stack.Header("Custom Poké Mart Item");

            var martItemOptions = metadata.IsFireRedOrLeafGreen ? allItems.Where(i => !ItemUtils.IsTM(i) && !ItemUtils.IsHM(i)).ToList() : allItems;
            var martItemDisplay = martItemOptions.Select(EnumUtils.ToDisplayString).ToList();

            var customMartItemCb = stack.Add(new BoundCheckBoxUI("Add Custom Item To Poké Marts", model.AddItemToPokemarts));
            var customMartItemStack = customMartItemCb.BindVisibility(stack.Add(CreateStack()));
            customMartItemStack.Add(new EnumComboBoxUI<Item>("Item to Add", martItemDisplay, model.CustomMartItem, martItemOptions));
            if (metadata.IsFireRedOrLeafGreen)
            {
                customMartItemStack.Add(new Label() { Content = "NOTE: TMs and HMs cannot be added as custom shop items in FRLG, as shops with TMs or HMs that also have any other items crash the game" });
            }
            var modifyMartItemPriceCb = customMartItemStack.Add(new BoundCheckBoxUI("Override Item Price", model.OverrideCustomMartItemPrice));
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
            var separateHiddenItemsCb = new BoundCheckBoxUI("Use Separate Settings for Hidden Items", model.UseSeperateHiddenItemSettings);
            separateHiddenItemsCb.BindVisibility(hiddenItemStack);
            stack.Add(separateHiddenItemsCb, hiddenItemStack);
            return CreateTabItem("Field and Gift Items", stack);
        }
    }
}
