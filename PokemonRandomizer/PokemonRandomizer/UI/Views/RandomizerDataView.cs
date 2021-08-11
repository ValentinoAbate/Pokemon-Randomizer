namespace PokemonRandomizer.UI.Views
{
    using Models;
    using System.Windows.Controls;

    public class RandomizerDataView : DataView<RandomizerDataModel>
    {
        public RandomizerDataView(RandomizerDataModel model)
        {
            var stack = new StackPanel() { Orientation = Orientation.Vertical };
            var useSeedCb = new BoundCheckBoxUI(model.UseSeed, "Use Seed");
            var seedTb = new BoundTextBoxUI("Seed", model.Seed);
            useSeedCb.BindVisibility(seedTb);
            stack.Add(useSeedCb, seedTb);
            Content = stack;
        }
    }
}
