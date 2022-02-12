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
            stack.Add(new BoundCheckBoxUI(model.UpdateDOTMoves, "Update Wrap Moves", "Updates the moves Wrap, Bind, Fire Spin, Sand Tomb, Whirlpool, and Clamp to their Gen V power, accuracy, and PP"));
            stack.Header("Randomizer Options");
            stack.Add(new BoundCheckBoxUI(model.CountRelicanthAsFossil, "Count Relicanth as a Fossil Pokemon"));
            if (metadata.IsEmerald)
            {
                stack.Header("Emerald Options");
                stack.Add(new BoundCheckBoxUI(model.RandomizeWallyAce, "Randomize Catching Tutorial Pokemon / Wally Ace"));
                stack.Add(new BoundCheckBoxUI(model.EasyFirstRivalbattle, "Easy First Rival Battle") { ToolTip = easyRivalBattleTooltip });
                var berryRand = stack.Add(new RandomChanceUI("Randomize Berry Trees", model.RandomizeBerryTrees, model.BerryTreeRandomizationChance) { ToolTip = "Randomize which berry trees start in the berry tree slots. WARNING: this only works for new save files. Loading a save state of an old save file will bypass the script that sets the starting berry trees." });
                var berryStack = stack.Add(berryRand.BindEnabled(new StackPanel{ Orientation = Orientation.Horizontal }));
                berryStack.Add(new BoundCheckBoxUI(model.BanEvBerries, "Ban EV Berries") { ToolTip = "Prevent EV-Lowering berries from appearing in randomized berry trees" });
                berryStack.Add(new BoundCheckBoxUI(model.BanMinigameBerries, "Ban Pokéblock / Minigame Berries") { ToolTip = "Prevent Pokéblock / Minigame berries from appearing in randomized berry trees" });
            }
            else if (metadata.IsFireRed)
            {
                stack.Header("Fire Red Options");
                stack.Add(new BoundCheckBoxUI(model.EvolveWithoutNationalDex, "Evolve Without National Dex") { ToolTip = evolveWithoutNatDexTooltip });
            }
        }
    }
}
