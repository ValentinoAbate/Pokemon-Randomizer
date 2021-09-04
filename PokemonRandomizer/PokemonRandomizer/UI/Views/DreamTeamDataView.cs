using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
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

        private static List<WeightedSetUI<PokemonType>.ChoiceBoxItem> GetTypeDropdown() => new List<WeightedSetUI<PokemonType>.ChoiceBoxItem>
        {
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = (PokemonType)19, Content="None"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.NRM, Content="Normal"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.FTG, Content="Fighting"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.FLY, Content="Flying"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.PSN, Content="Poison"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.GRD, Content="Ground"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.RCK, Content="Rock"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.BUG, Content="Bug"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.GHO, Content="Ghost"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.STL, Content="Steel"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.FIR, Content="Fire"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.WAT, Content="Water"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.GRS, Content="Grass"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.ELE, Content="Electric"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.PSY, Content="Psychic"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.ICE, Content="Ice"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.DRG, Content="Dragon"},
            new WeightedSetUI<PokemonType>.ChoiceBoxItem { Item = PokemonType.DRK, Content="Dark"},
        };
        private static List<WeightedSetUI<PokemonType>.ChoiceBoxItem> ReferenceDropdown {get; set;} = GetTypeDropdown();
                                                                              
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
            var typeFilterCb = genStack.Add(new BoundCheckBoxUI(model.UseTypeFilter, "Type Limitiation"));
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 1", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType1.Value), i => model.AllowedType1.Value = ReferenceDropdown[i].Item)));
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 2", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType2.Value), i => model.AllowedType2.Value = ReferenceDropdown[i].Item)));
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 3", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType3.Value), i => model.AllowedType3.Value = ReferenceDropdown[i].Item)));
            genStack.Add(new BoundCheckBoxUI(model.BanLegendaries, "Ban Legendaries"));
            genStack.Add(new BoundCheckBoxUI(model.BanIllegalEvolutions, "Ban Illegal Evolutions"));
            optionCb.BindVisibility(genStack, (int)DreamTeamSetting.Random);
        }
    }
}
