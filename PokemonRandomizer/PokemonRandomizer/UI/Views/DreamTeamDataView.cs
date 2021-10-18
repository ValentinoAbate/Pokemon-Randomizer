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

        private static List<TypedComboBoxItem<PokemonType>> GetTypeDropdown() => new List<TypedComboBoxItem<PokemonType>>
        {
            new TypedComboBoxItem<PokemonType> { Item = (PokemonType)19, Content="None"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.NRM, Content="Normal"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.FTG, Content="Fighting"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.FLY, Content="Flying"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.PSN, Content="Poison"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.GRD, Content="Ground"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.RCK, Content="Rock"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.BUG, Content="Bug"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.GHO, Content="Ghost"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.STL, Content="Steel"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.FIR, Content="Fire"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.WAT, Content="Water"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.GRS, Content="Grass"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.ELE, Content="Electric"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.PSY, Content="Psychic"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.ICE, Content="Ice"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.DRG, Content="Dragon"},
            new TypedComboBoxItem<PokemonType> { Item = PokemonType.DRK, Content="Dark"},
        };
        private static List<TypedComboBoxItem<PokemonType>> ReferenceDropdown {get; set;} = GetTypeDropdown();
                                                                              
        public DreamTeamDataView(DreamTeamDataModel model, string[] pokemonNames, List<Pokemon> pokemon)
        { 
            var stack = CreateMainStack();
            stack.Header("Dream Team");
            stack.Description("Dream Team will put 6 Pokemon in the first route of the game. You can select custom pokemon, or randomly generate a team!");
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
