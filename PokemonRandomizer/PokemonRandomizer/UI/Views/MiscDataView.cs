using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using Backend.DataStructures;

    public class MiscDataView : DataView<MiscDataModel>
    {
        private const string easyRivalBattleTooltip = "Sets the first rival battle to have level 1 pokemon. Useful for randomized starters (this battle must be cleared in RSE)";
        private const string evolveWithoutNatDexTooltip = "Allow pokemon to evolve without needing national dex in FRLG. Currently only works for level-up evolutions. Will be replaced with starting with the national pokedex eventually";
        public MiscDataView(MiscDataModel model, RomMetadata metadata)
        {
            // Create stack and add content
            var stack = CreateStack();
            Content = stack;

            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            stack.Add(new BoundCheckBoxUI(model.RunIndoors, "Run Indoors"));
            stack.Header("Randomizer Options");
            stack.Add(new BoundCheckBoxUI(model.CountRelicanthAsFossil, "Count Relicanth as a Fossil Pokemon"));
            if (metadata.IsEmerald)
            {
                stack.Header("Emerald Options");
                stack.Add(new BoundCheckBoxUI(model.RandomizeWallyAce, "Randomize Catching Tutorial Pokemon / Wally Ace"));
                stack.Add(new BoundCheckBoxUI(model.EasyFirstRivalbattle, "Easy First Rival Battle") { ToolTip = easyRivalBattleTooltip });
            }
            else if (metadata.IsFireRed)
            {
                stack.Header("Fire Red Options");
                stack.Add(new BoundCheckBoxUI(model.EvolveWithoutNationalDex, "Evolve Without National Dex") { ToolTip = evolveWithoutNatDexTooltip });
            }
        }
    }
}
