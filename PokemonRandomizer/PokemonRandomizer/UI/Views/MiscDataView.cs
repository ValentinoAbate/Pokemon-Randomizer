using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    public class MiscDataView : DataView<MiscDataModel>
    {
        public MiscDataView(MiscDataModel model)
        {
            // Create stack and add content
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            Content = stack;

            stack.Add(new Label() { Content = UISkin.Current.HacksAndTweaksHeader });
            stack.Add(new Separator());
            stack.Add(new BoundCheckBoxUI(model.RunIndoors, "Run Indoors"));
            stack.Add(new BoundCheckBoxUI(model.EvolveWithoutNationalDex, "Evolve Without National Dex"));
            stack.Add(new Label() { Content = "Randomizer Options" });
            stack.Add(new Separator());
            stack.Add(new BoundCheckBoxUI(model.CountRelicanthAsFossil, "Count Relicanth as a Fossil Pokemon"));
        }
    }
}
