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
            var contentStack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = contentStack;
            contentStack.Children.Add(new Label() { Content = "Randomization" });
            contentStack.Children.Add(new Separator());
            contentStack.Children.Add(new RandomChanceUI("Tm Moves", model.TMRandChance, (d) => model.TMRandChance = d, Orientation.Horizontal));
            contentStack.Children.Add(new RandomChanceUI("Tutor Moves", model.MoveTutorRandChance, (d) => model.MoveTutorRandChance = d, Orientation.Horizontal));
            contentStack.Children.Add(new BoundCheckBoxUI((b) => model.PreventHmMovesInTMsAndMoveTutors = b) { Content = "Prevent HM moves in TMs and Tutors", IsChecked = model.PreventHmMovesInTMsAndMoveTutors });
            contentStack.Children.Add(new BoundCheckBoxUI((b) => model.PreventDuplicateTMsAndMoveTutors = b) { Content = "Prevent duplicate moves in TMs and Tutors", IsChecked = model.PreventDuplicateTMsAndMoveTutors });
            contentStack.Children.Add(new Separator());
            contentStack.Children.Add(new Label() { Content = "Compatibility" });
            contentStack.Children.Add(new Separator());
            contentStack.Children.Add(new LabeledComboBoxUI("TM Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.TmCompatOption, (i) => model.TmCompatOption = (TmHmTutorDataModel.CompatOption)i));
            contentStack.Children.Add(new LabeledComboBoxUI("Tutor Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.TutorCompatOption, (i) => model.TutorCompatOption = (TmHmTutorDataModel.CompatOption)i));
            contentStack.Children.Add(new LabeledComboBoxUI("HM Compatibility", TmHmTutorDataModel.CompatOptionDropdown, (int)model.HmCompatOption, (i) => model.HmCompatOption = (TmHmTutorDataModel.CompatOption)i));
            contentStack.Children.Add(new PercentSliderUI("Random Compatibility On Chance", model.RandomCompatTrueChance, (d) => model.RandomCompatTrueChance = d));
            contentStack.Children.Add(new PercentSliderUI("Intelligent Compatibiliy Noise", model.IntelligentCompatNoise, (d) => model.IntelligentCompatNoise = d, 0.01, 0, 0.33));
        }
    }
}
