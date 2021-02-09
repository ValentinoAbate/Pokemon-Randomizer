using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PokemonRandomizer.UI
{
    public class TmHmTutorDataView : DataView<TmHmTutorDataModel>
    {
        public TmHmTutorDataView(TmHmTutorDataModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Randomization" });
            stack.Add(new Separator());
            stack.Add(new RandomChanceUI("Tm Moves", model.TMRandChance, (d) => model.TMRandChance = d, Orientation.Horizontal));
            stack.Add(new RandomChanceUI("Tutor Moves", model.MoveTutorRandChance, (d) => model.MoveTutorRandChance = d, Orientation.Horizontal));
            stack.Add(new BoundCheckBoxUI((b) => model.PreventHmMovesInTMsAndMoveTutors = b) { Content = "Prevent HM moves in TMs and Tutors", IsChecked = model.PreventHmMovesInTMsAndMoveTutors });
            stack.Add(new BoundCheckBoxUI((b) => model.PreventDuplicateTMsAndMoveTutors = b) { Content = "Prevent duplicate moves in TMs and Tutors", IsChecked = model.PreventDuplicateTMsAndMoveTutors });
            stack.Add(new BoundCheckBoxUI((b) => model.KeepImportantTms = b, "Ensures that important TMs won't be randomized (Headbutt, Rock Smash, Secret Power, Flash, etc)") { Content = "Keep important TMs", IsChecked = model.KeepImportantTms });
            stack.Add(new Separator());
            stack.Add(new Label() { Content = "Compatibility" });
            stack.Add(new Separator());
            stack.Add(new BoundComboBoxUI("TM Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.TmCompatOption, (i) => model.TmCompatOption = (TmHmTutorDataModel.CompatOption)i));
            stack.Add(new BoundComboBoxUI("Tutor Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.TutorCompatOption, (i) => model.TutorCompatOption = (TmHmTutorDataModel.CompatOption)i));
            stack.Add(new BoundComboBoxUI("HM Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.HmCompatOption, (i) => model.HmCompatOption = (TmHmTutorDataModel.CompatOption)i));
            stack.Add(new PercentSliderUI("Random Compatibility On Chance", model.RandomCompatTrueChance, (d) => model.RandomCompatTrueChance = d));
            stack.Add(new PercentSliderUI("Intelligent Compatibiliy Noise", model.IntelligentCompatNoise, (d) => model.IntelligentCompatNoise = d, 0.01, 0, 0.33));
        }
    }
}
