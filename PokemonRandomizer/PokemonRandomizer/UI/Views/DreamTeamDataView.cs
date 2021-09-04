using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.UI.Models;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Views
{
    public class DreamTeamDataView : DataView<DreamTeamDataModel>
    {
        public CompositeCollection DreamTeamOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="No Dream Team" },
            new ComboBoxItem() {Content="Custom Dream Team" },
            new ComboBoxItem() {Content="Generate Dream Team", ToolTip="Generates 6 Pokemon based on settings such as BST limits, allowed types, etc."},
        };
        public CompositeCollection DreamTeamBstOption => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None" },
            new ComboBoxItem() {Content="Fully Evolved Base Stat Total Must be Greater Than Limit" },
            new ComboBoxItem() {Content="Fully Evolved Base Stat Total Must be Less Than Limit" },
        };

        public DreamTeamDataView(DreamTeamDataModel model, string[] pokemonNames, List<Pokemon> pokemon)
        {
            var stack = CreateMainStack();
            stack.Header("Dream Team");
            stack.Add(new Label() { Content = "Dream Team will put 6 Pokemon in the first route of the game. You can select custom pokemon, or randomly generate a team!" });
            stack.Add(new Separator());
            var optionCb = stack.Add(new EnumComboBoxUI<DreamTeamSetting>("Dream Team Strategy", DreamTeamOptionDropdown, model.DreamTeamOption));

            // Custom Team UI
            var pokemonOptions = new List<string>(pokemonNames.Length + 1) { "None" };
            pokemonOptions.AddRange(pokemonNames);
            BoundComboBoxUI CustomStarterCB(int index)
            {
                return new BoundComboBoxUI("", pokemonOptions, pokemon.IndexOf(model.CustomDreamTeam[index]), i => model.CustomDreamTeam[index] = pokemon[i]);
            }
            var customStack = stack.Add(new StackPanel() { Orientation = Orientation.Horizontal });
            customStack.Add(new Label() { Content = "Custom Team:" }, CustomStarterCB(0), CustomStarterCB(1), CustomStarterCB(2), CustomStarterCB(3), CustomStarterCB(4), CustomStarterCB(5));
            optionCb.BindVisibility(customStack, (int)DreamTeamSetting.Custom);

            var genStack = stack.Add(CreateStack());
            var useTotalBstCb = genStack.Add(new EnumComboBoxUI<DreamTeamBstTotalOption>("Base State Total Limitation", DreamTeamBstOption, model.UseTotalBST));
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Base Stat Total Lower Limit", model.BstTotalLowerBound, false, 50, 1900, 4050)), (int)DreamTeamBstTotalOption.Min);
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Base Stat Total Upper Limit", model.BstTotalUpperBound, false, 50, 1900, 4050)), (int)DreamTeamBstTotalOption.Max);
            genStack.Add(new BoundCheckBoxUI(model.BanLegendaries, "Ban Legendaries"));
            genStack.Add(new BoundCheckBoxUI(model.BanIllegalEvolutions, "Ban Illegal Evolutions"));
            optionCb.BindVisibility(genStack, (int)DreamTeamSetting.Random);
        }
    }
}
