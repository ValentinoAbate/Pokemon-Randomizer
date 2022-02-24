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
        private static CompositeCollection CompatOptionDropdown => new()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="Full Compatibility", ToolTip="All pokemon will be compatible with all TMs, HMs, and Tutors" },
            new ComboBoxItem() {Content="Random", ToolTip="Randomly select TM and Tutor compatibility. Does not randomize HM compatibility" },
            new ComboBoxItem() {Content="Random (Keep Number)", ToolTip="Randomly select TM and Tutor compatibility, but keep the same number the pokemon had in the base ROM. Does not randomize HM compatibility"},
            new ComboBoxItem() {Content="Intelligent (Recommended)", ToolTip=intelligentCompatTooltip},
        };

        public TmHmTutorDataView(TmHmTutorModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Randomization" });
            stack.Add(new Separator());
            stack.Add(new RandomChanceUI("Random TM Moves", model.RandomizeTMs, model.TMRandChance));
            stack.Add(new RandomChanceUI("Random Tutor Moves", model.RandomizeMoveTutors, model.MoveTutorRandChance));
            stack.Add(new BoundCheckBoxUI(model.NoHmMovesInTMsAndTutors, "Prevent HM moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.NoDuplicateTMsAndTutors, "Prevent duplicate moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.KeepImportantTmsAndTutors, "Keep important TMs and Tutors", "Ensures that important TMs and Tutors won't be randomized (Secret Power + Dig in RSE)"));
            stack.Add(new Separator());
            stack.Add(new Label() { Content = "Compatibility" });
            stack.Add(new Separator());
            var compatDropdown = stack.Add(new EnumComboBoxUI<CompatOption>("Compatibility Strategy", CompatOptionDropdown, model.MoveCompatOption));
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Random Compatibility Chance", model.RandomCompatTrueChance), (int)CompatOption.Random, (int)CompatOption.RandomKeepNumber));
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Intelligent Compatibiliy Random Chance (NORMAL Moves)", model.IntelligentCompatNormalRandChance, true, 0.01, 0, 0.5) { ToolTip = intelligentRandomNormalTooltip}, (int)CompatOption.Intelligent));
            stack.Add(compatDropdown.BindVisibility(new BoundSliderUI("Intelligent Compatibiliy Random Chance (Other)", model.IntelligentCompatNoise, true, 0.01, 0, 0.33) { ToolTip = intelligentRandomTooltip }, (int)CompatOption.Intelligent));
            stack.Add(new BoundCheckBoxUI(model.ForceFullHmCompat, "Force Full HM Compatibility", "Forces all pokemon to be compatible with all HMs, regardless of other compatibility settings"));
        }
    }
}
