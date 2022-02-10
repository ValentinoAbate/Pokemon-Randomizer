using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using CompatOption = Settings.MoveCompatOption;
    public class TmHmTutorDataView : DataView<TmHmTutorModel>
    {
        private CompositeCollection CompatOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="All On" },
            new ComboBoxItem() {Content="Random" },
            new ComboBoxItem() {Content="Random (Keep Number)", ToolTip="Randomly select compatibility, but keep the same number the pokemon had in the base ROM"},
            new ComboBoxItem() {Content="Intelligent", ToolTip=""},
        };

        private CompositeCollection HmCompatOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Unchanged" },
            new ComboBoxItem() {Content="All On" },
            new ComboBoxItem() {Content="Random (WARNING: UNSAFE)", ToolTip="WARNING: Randomizing HM compatibility may create ROMs that cannot be completed" },
            new ComboBoxItem() {Content="Random - Keep Number (WARNING: UNSAFE)", ToolTip="WARNING: Randomizing HM compatibility may create ROMs that cannot be completed. Randomly select compatibility, but keep the same number the pokemon had in the base ROM"},
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
            stack.Add(new BoundCheckBoxUI(model.KeepImportantTmsAndTutors, "Keep important TMs and Tutors", "Ensures that important TMs and Tutors won't be randomized (Headbutt, Rock Smash, Secret Power, Flash, etc)"));
            stack.Add(new Separator());
            stack.Add(new Label() { Content = "Compatibility" });
            stack.Add(new Separator());
            stack.Add(new EnumComboBoxUI<CompatOption>("TM Compatibility", CompatOptionDropdown, model.TmCompatOption));
            stack.Add(new EnumComboBoxUI<CompatOption>("Tutor Compatibility", CompatOptionDropdown, model.TutorCompatOption));
            stack.Add(new EnumComboBoxUI<CompatOption>("HM Compatibility", HmCompatOptionDropdown, model.HmCompatOption));
            stack.Add(new BoundSliderUI("Random Compatibility On Chance", model.RandomCompatTrueChance));
            stack.Add(new BoundSliderUI("Intelligent Compatibiliy Noise", model.IntelligentCompatNoise, true, 0.01, 0, 0.33));
        }
    }
}
