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
    public class TmHmTutorDataView : DataView<TmHmTutorModel>
    {
        public TmHmTutorDataView(TmHmTutorModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;
            stack.Add(new Label() { Content = "Randomization" });
            stack.Add(new Separator());
            stack.Add(new RandomChanceUI("Random Tm Moves", model.TMRandChance, (d) => model.TMRandChance = d, Orientation.Horizontal));
            stack.Add(new RandomChanceUI("Random Tutor Moves", model.MoveTutorRandChance, (d) => model.MoveTutorRandChance = d, Orientation.Horizontal));
            stack.Add(new BoundCheckBoxUI(model.NoHmMovesInTMsAndMoveTutors, (b) => model.NoHmMovesInTMsAndMoveTutors = b, "Prevent HM moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.NoDuplicateTMsAndMoveTutors, (b) => model.NoDuplicateTMsAndMoveTutors = b, "Prevent duplicate moves in TMs and Tutors"));
            stack.Add(new BoundCheckBoxUI(model.KeepImportantTms, (b) => model.KeepImportantTms = b, "Keep important TMs", "Ensures that important TMs won't be randomized (Headbutt, Rock Smash, Secret Power, Flash, etc)"));
            stack.Add(new Separator());
            stack.Add(new Label() { Content = "Compatibility" });
            stack.Add(new Separator());
            stack.Add(new BoundComboBoxUI("TM Compatibility", TmHmTutorModel.CompatOptionDropdown, (int)model.TmCompatOption, (i) => model.TmCompatOption = (TmHmTutorModel.CompatOption)i));
            stack.Add(new BoundComboBoxUI("Tutor Compatibility", TmHmTutorModel.CompatOptionDropdown, (int)model.TutorCompatOption, (i) => model.TutorCompatOption = (TmHmTutorModel.CompatOption)i));
            stack.Add(new BoundComboBoxUI("HM Compatibility", TmHmTutorModel.CompatOptionDropdown, (int)model.HmCompatOption, (i) => model.HmCompatOption = (TmHmTutorModel.CompatOption)i));
            stack.Add(new BoundSliderUI("Random Compatibility On Chance", model.RandomCompatTrueChance, (d) => model.RandomCompatTrueChance = d));
            stack.Add(new BoundSliderUI("Intelligent Compatibiliy Noise", model.IntelligentCompatNoise, (d) => model.IntelligentCompatNoise = d, true, 0.01, 0, 0.33));
        }
    }
}
