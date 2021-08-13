using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;
    using PokemonRandomizer.Backend.Utilities;
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
            var tab = new TabItem() { Header = "Misc" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = "PC Potion Randomization" });
            stack.Add(new Separator());
            var strategyCb = stack.Add(new EnumComboBoxUI<PcItemOption>("Randomization Strategy", PCPotionStrategyDropdown, model.PcPotionOption));
            strategyCb.BindVisibility(stack.Add(new ItemSettingsUI(model.PcItemSettings, false)), (int)PcItemOption.Random);
            strategyCb.BindVisibility(stack.Add(new EnumComboBoxUI<Item>("Custom PC Potion Item", EnumUtils.GetDisplayValues<Item>(), model.CustomPcItem)), (int)PcItemOption.Custom);
            tab.Content = stack;
            return tab;
        }

        private TabItem CreateShopsTab(ItemDataModel model)
        {
            var tab = new TabItem() { Header = "Shops" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            tab.IsEnabled = false;
            return tab;
        }

        private TabItem CreateFieldItemsTab(ItemDataModel model)
        {
            var tab = new TabItem() { Header = "Field and Gift Items" };
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Add(new Label() { Content = "Field And Gift Item Randomization" });
            stack.Add(new Separator());
            var fieldItemSettings = new ItemSettingsUI(model.FieldItemSettings, false);
            stack.Add(new RandomChanceUI("Randomize Field and Gift Items", model.RandomizeFieldItems, model.FieldItemRandChance, fieldItemSettings));
            stack.Add(fieldItemSettings);
            stack.Add(new Label() { Content = "Hidden Item Randomization" });
            stack.Add(new Separator());
            var hiddenItemStack = new StackPanel() { Orientation = Orientation.Vertical };
            var hiddenItemSettings = new ItemSettingsUI(model.HiddenItemSettings, false);
            hiddenItemStack.Add(new RandomChanceUI("Randomize Hidden Items", model.RandomizeHiddenItems, model.HiddenItemRandChance, hiddenItemSettings));
            hiddenItemStack.Add(hiddenItemSettings);
            var separateHiddenItemsCb = new BoundCheckBoxUI(model.UseSeperateHiddenItemSettings, "Use Separate Settings for Hidden Items");
            separateHiddenItemsCb.BindVisibility(hiddenItemStack);
            stack.Add(separateHiddenItemsCb, hiddenItemStack);
            tab.Content = stack;
            return tab;
        }
    }
}
