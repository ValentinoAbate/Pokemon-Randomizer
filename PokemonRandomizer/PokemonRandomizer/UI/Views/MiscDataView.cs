using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using Backend.DataStructures;

    public class MiscDataView : DataView<MiscDataModel>
    {
        public MiscDataView(MiscDataModel model, RomMetadata metadata)
        {
            // Create stack and add content
            var stack = CreateStack();
            Content = stack;

            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            stack.Add(new BoundCheckBoxUI(model.RunIndoors, "Run Indoors"));
            stack.Add(new BoundCheckBoxUI(model.EvolveWithoutNationalDex, "Evolve Without National Dex"));
            stack.Header("Randomizer Options");
            stack.Add(new BoundCheckBoxUI(model.CountRelicanthAsFossil, "Count Relicanth as a Fossil Pokemon"));
            if (metadata.IsEmerald)
            {
                stack.Header("Emerald Options");
                stack.Add(new BoundCheckBoxUI(model.RandomizeWallyAce, "Randomize Catching Tutorial Pokemon / Wally Ace"));
            }
        }
    }
}
