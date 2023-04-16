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
        private const string dreamTeamOptionTooltip = "The strategy used to choose the pokemon included in the Dream Team";
        private const string customPokemonTooltip = "The 1-6 custom pokemon that will be included in the dream team";
        private const string bstLimitTooltip = "Restricts the pokemon that are chosen based on a configurable base stat total minimum or maximum" +
            "\nBase stat total limitations always use a pokemon's fully evolved base stat total, and can apply to individual pokemon or to the party base stat total";
        private const string partyBstMinTooltip = "The sum of the base stat totals of each pokemon in the Dream Team (when fully evolved) will be greater than or equal to the minimum value (when possible)";
        private const string partyBstMaxTooltip = "The sum of the base stat totals of each pokemon in the Dream Team (when fully evolved) will be less than or equal to the maximum value (when possible)";
        private const string individualBstMinTooltip = "The base stat total of each pokemon in the Dream Team (when fully evolved) will be greater than or equal to the minimum value (when possible)";
        private const string individualBstMaxTooltip = "The base stat total of each pokemon in the Dream Team (when fully evolved) will be less than or equal to the maximum value (when possible)";
        public CompositeCollection DreamTeamOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="No Dream Team", ToolTip="No Dream Team will be generated" },
            new ComboBoxItem() {Content="Custom Dream Team", ToolTip="Choose a team of 1-6 custom pokemon" },
            new ComboBoxItem() {Content="Generate Dream Team", ToolTip="Generate a team of 6 pokemon based on settings such as base stat total limits, allowed types, etc."},
        };
        public CompositeCollection DreamTeamBstOption => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="No base stat total limits will be applied to the Dream Team" },
            new ComboBoxItem() {Content="Party Fully Evolved Base Stat Total Minimum", ToolTip=partyBstMinTooltip },
            new ComboBoxItem() {Content="Party Fully Evolved Base Stat Total Maximum", ToolTip=partyBstMaxTooltip },
            new ComboBoxItem() {Content="Individual Fully Evolved Base Stat Total Minimum", ToolTip=individualBstMinTooltip },
            new ComboBoxItem() {Content="Individual Fully Evolved Base Stat Total Maximum", ToolTip=individualBstMaxTooltip },
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
            var optionCb = stack.Add(new EnumComboBoxUI<DreamTeamSetting>("Dream Team Strategy", DreamTeamOptionDropdown, model.DreamTeamOption) { ToolTip = dreamTeamOptionTooltip });

            // Custom Team UI
            var pokemonOptions = new List<string>(pokemonNames.Length + 1) { "None" };
            pokemonOptions.AddRange(pokemonNames);
            BoundComboBoxUI CustomStarterCB(int index)
            {
                return new BoundComboBoxUI("", pokemonOptions, pokemon.IndexOf(model.CustomDreamTeam[index]), i => model.CustomDreamTeam[index] = pokemon[i]);
            }
            var customStack = stack.Add(new StackPanel() { Orientation = Orientation.Horizontal });
            var customPokemonLabel = new Label() { Content = "Custom Team:", ToolTip = customPokemonTooltip };
            customStack.Add(customPokemonLabel, CustomStarterCB(0), CustomStarterCB(1), CustomStarterCB(2), CustomStarterCB(3), CustomStarterCB(4), CustomStarterCB(5));
            optionCb.BindVisibility(customStack, (int)DreamTeamSetting.Custom);

            var genStack = stack.Add(CreateStack());
            var useTotalBstCb = genStack.Add(new EnumComboBoxUI<DreamTeamBstTotalOption>("Base Stat Total Limitation", DreamTeamBstOption, model.UseTotalBST) { ToolTip = bstLimitTooltip});
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Party Base Stat Total Minimum", model.BstTotalLowerBound, false, 50, 1900, 4050)), (int)DreamTeamBstTotalOption.TotalMin);
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Party Base Stat Total Maximum", model.BstTotalUpperBound, false, 50, 1900, 4050)), (int)DreamTeamBstTotalOption.TotalMax);
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Individual Base Stat Total Minimum", model.BstIndividualLowerBound, false, 10, 400, 600)), (int)DreamTeamBstTotalOption.IndividualMin);
            useTotalBstCb.BindVisibility(genStack.Add(new BoundSliderUI("Individual Base Stat Total Maximum", model.BstIndividualUpperBound, false, 10, 400, 600)), (int)DreamTeamBstTotalOption.IndividualMax);
            var typeFilterCb = genStack.Add(new BoundCheckBoxUI("Type Limitiation", model.UseTypeFilter) { ToolTip="Limit which type(s) of pokemon can appear in the Dream Team" });
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 1", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType1.Value), i => model.AllowedType1.Value = ReferenceDropdown[i].Item)));
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 2", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType2.Value), i => model.AllowedType2.Value = ReferenceDropdown[i].Item)));
            typeFilterCb.BindVisibility(genStack.Add(new BoundComboBoxUI("Allowed Type 3", GetTypeDropdown(), ReferenceDropdown.FindIndex(i => i.Item == model.AllowedType3.Value), i => model.AllowedType3.Value = ReferenceDropdown[i].Item)));
            genStack.Add(new BoundCheckBoxUI("Prioritize Variants", model.PrioritizeVariants, "Choose Variant Pokemon first when possible"));
            genStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendaries, "Prevent legendary pokemon from appearing in the Dream Team"));
            genStack.Add(new BoundCheckBoxUI("Ban Illegal Evolutions", model.BanIllegalEvolutions, "Ensure that pokemon in the Dream Team appear at a legal evolution stage for the level they appear at"));
            optionCb.BindVisibility(genStack, (int)DreamTeamSetting.Random);
        }
    }
}
