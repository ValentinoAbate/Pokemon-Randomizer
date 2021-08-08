using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    using CompatOption = Settings.TmMtCompatOption;
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
        public TmHmTutorDataView(TmHmTutorModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Randomization" });
            stack.Add(new Separator());
            stack.Add(new RandomChanceUI("Random TM Moves", model.RandomizeTMs, b => model.RandomizeTMs = b, model.TMRandChance, d => model.TMRandChance = d));
            stack.Add(new RandomChanceUI("Random Tutor Moves", model.RandomizeMoveTutors, b => model.RandomizeMoveTutors = b, model.MoveTutorRandChance, d => model.MoveTutorRandChance = d));
            stack.Add(new BoundCheckBoxUI(model.NoHmMovesInTMsAndTutors, b => model.NoHmMovesInTMsAndTutors = b, "Prevent HM moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.NoDuplicateTMsAndTutors, b => model.NoDuplicateTMsAndTutors = b, "Prevent duplicate moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.KeepImportantTmsAndTutors, b => model.KeepImportantTmsAndTutors = b, "Keep important TMs and Tutors", "Ensures that important TMs and Tutors won't be randomized (Headbutt, Rock Smash, Secret Power, Flash, etc)"));
            stack.Add(new Separator());
            stack.Add(new Label() { Content = "Compatibility" });
            stack.Add(new Separator());
            stack.Add(new BoundComboBoxUI("TM Compatibility", CompatOptionDropdown, (int)model.TmCompatOption, (i) => model.TmCompatOption = (CompatOption)i));
            stack.Add(new BoundComboBoxUI("Tutor Compatibility", CompatOptionDropdown, (int)model.TutorCompatOption, (i) => model.TutorCompatOption = (CompatOption)i));
            stack.Add(new BoundComboBoxUI("HM Compatibility", CompatOptionDropdown, (int)model.HmCompatOption, (i) => model.HmCompatOption = (CompatOption)i));
            stack.Add(new BoundSliderUI("Random Compatibility On Chance", model.RandomCompatTrueChance, (d) => model.RandomCompatTrueChance = d));
            stack.Add(new BoundSliderUI("Intelligent Compatibiliy Noise", model.IntelligentCompatNoise, (d) => model.IntelligentCompatNoise = d, true, 0.01, 0, 0.33));
        }
    }
}
