using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using CompatOption = Settings.MoveCompatOption;
    public class TmHmTutorDataView : DataView<TmHmTutorModel>
    {
        private const string intelligentCompatTooltip = "Intelligently determine TM/HM/Tutor compatibility. Moves that are the same type as the pokemon, are in the pokemon's learnset or egg moves, or were originally learnable by the pokemon through TMs/HMs/Tutors will be compatible" +
            "\nMoves that don't meet any of those requirements and are known to be incompatible with the pokemon (from the original TM/Tutor data) will be incompatible" +
            "\nAny moves that aren't explicitly determined to be compatible or incompatible by the above logic will have their compatibility randomly determined, using the chances defined by the intelligent random chance settings" +
            "\nHM Compatibility will only be recalculated for Type-Variant Pokemon" +
            "\nPokemon that were designed to learn no TMs/HMs/Tutors (such as Magikarp) will be incompatible with any moves that are not in their learnset" +
            "\nPokemon that were designed to learn all TMs/HMs/Tutors (such as Mew) will be compatible with all moves";

        private const string intelligentRandomTooltip = "The chance that any non-NORMAL move whose compatibility can't be explicitly determined will be compatible";
        private const string intelligentRandomNormalTooltip = "The chance that any NORMAL move whose compatibility can't be explicitly determined will be compatible";
        private const string compatOptionTooltip = "The strategy used to determing TM/HM/Tutor compatibility" +
            "\nRead the tooltips of the options in the dropdown menu for more info on how they work";
        private static CompositeCollection CompatOptionDropdown => new()
        {
            new ComboBoxItem() {Content="Unchanged", ToolTip="Compatiblity will be left as it is in the base game. This may cause compatibility to not make sense if TM or Tutors are randomized and for Variant Pokemon" },
            new ComboBoxItem() {Content="Full Compatibility", ToolTip="All pokemon will be compatible with all TMs, HMs, and Tutors" },
            new ComboBoxItem() {Content="Random", ToolTip="Randomly select TM and Tutor compatibility. Does not randomize HM compatibility" },
            new ComboBoxItem() {Content="Random (Keep Number)", ToolTip="Randomly select TM and Tutor compatibility, but keep the same number the pokemon had in the base ROM. Does not randomize HM compatibility"},
            new ComboBoxItem() {Content="Intelligent (Recommended)", ToolTip=intelligentCompatTooltip},
        };

        public TmHmTutorDataView(TmHmTutorModel model)
        {
            var stack = CreateMainStack();
            stack.Header("Tm/Tutor Move Randomization");
            stack.Add(new RandomChanceUI("Random TM Moves", model.RandomizeTMs, model.TMRandChance) { ToolTip = "Randomize moves taught by TMs" });
            stack.Add(new RandomChanceUI("Random Tutor Moves", model.RandomizeMoveTutors, model.MoveTutorRandChance) { ToolTip = "Randomize moves taught by Move Tutor NPCs" });
            stack.Add(new BoundCheckBoxUI("Prevent HM moves in TMs and Tutors", model.NoHmMovesInTMsAndTutors, "Prevents HM moves from appearing as TMs or Move Tutors"));
            stack.Add(new BoundCheckBoxUI("Prevent duplicate moves in TMs and Tutors", model.NoDuplicateTMsAndTutors, "Ensures all TMs and Move Tutors are unique"));
            stack.Add(new BoundCheckBoxUI("Keep important TMs and Tutors", model.KeepImportantTmsAndTutors, "Ensures that important TMs and Tutors won't be randomized (Secret Power + Dig in RSE)"));
            stack.Separator();
            stack.Header("Tm/Hm/Tutor Compatibility");
            var compatDropdown = stack.Add(new EnumComboBoxUI<CompatOption>("Compatibility Strategy", CompatOptionDropdown, model.MoveCompatOption) { ToolTip = compatOptionTooltip });
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Random Compatibility Chance", model.RandomCompatTrueChance), (int)CompatOption.Random, (int)CompatOption.RandomKeepNumber));
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Intelligent Compatibiliy Random Chance (NORMAL Moves)", model.IntelligentCompatNormalRandChance, true, 0.01, 0, 0.5) { ToolTip = intelligentRandomNormalTooltip}, (int)CompatOption.Intelligent));
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Intelligent Compatibiliy Random Chance (Other)", model.IntelligentCompatNoise, true, 0.01, 0, 0.33) { ToolTip = intelligentRandomTooltip }, (int)CompatOption.Intelligent));
            stack.Add(new BoundCheckBoxUI("Force Full HM Compatibility", model.ForceFullHmCompat, "Forces all pokemon to be compatible with all HMs, regardless of other compatibility settings"));
        }
    }
}
