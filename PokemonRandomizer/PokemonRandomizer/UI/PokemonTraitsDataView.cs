using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PokemonRandomizer.UI
{
    public class PokemonTraitsDataView : GroupDataView<PokemonTraitsDataModel>
    {
        public override PokemonTraitsDataModel CloneModel(PokemonTraitsDataModel model)
        {
            throw new NotImplementedException();
        }

        public override Panel CreateModelView(PokemonTraitsDataModel model)
        {
            var grid = new Grid();
            //grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            var tabs = new TabControl();
            tabs.SetValue(Grid.RowProperty, 0);
            grid.Children.Add(tabs);

            tabs.Items.Add(CreateTypesTab(model));
            tabs.Items.Add(new TabItem() { Header = "Evolutions" });

            return grid;
        }

        private TabItem CreateTypesTab(PokemonTraitsDataModel model)
        {
            var tab = new TabItem() { Header = "Types" };
            var content = new Grid();
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            stack.Children.Add(new RandomChanceUI("Single Type", model.SingleTypeRandChance, (d) => model.SingleTypeRandChance = d, Orientation.Horizontal));
            stack.Children.Add(new RandomChanceUI("Dual Type (Primary)", model.DualTypePrimaryRandChance, (d) => model.DualTypePrimaryRandChance = d, Orientation.Horizontal));
            stack.Children.Add(new RandomChanceUI("Dual Type (Secondary)", model.DualTypeSecondaryRandChance, (d) => model.DualTypeSecondaryRandChance = d, Orientation.Horizontal));
            content.Children.Add(stack);
            tab.Content = content;
            return tab;
        }
    }
}
