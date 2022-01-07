using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;
    using PokemonRandomizer.Backend.Utilities;
    using System.Linq;
    using static Settings;

    public class ItemDataView : DataView<ItemDataModel>
    {
        public ItemDataView(ItemDataModel model)
        {
            var tabs = new TabControl();

            tabs.Items.Add(CreateFieldItemsTab(model));
            //tabs.Items.Add(CreateShopsTab(model));
            tabs.Items.Add(CreateMiscTab(model));

            Content = tabs;
        }

        public CompositeCollection PCPotionStrategyDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged"},
            new ComboBoxItem() {Content="Random"},
            new ComboBoxItem() {Content="Custom"},
        };

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
