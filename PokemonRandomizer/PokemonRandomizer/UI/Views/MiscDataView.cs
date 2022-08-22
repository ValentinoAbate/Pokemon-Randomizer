using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using Backend.DataStructures;
    using System.Windows.Data;

    public class MiscDataView : DataView<MiscDataModel>
    {
        private const string easyRivalBattleTooltip = "Sets the first rival battle to have level 1 pokemon. Useful for randomized starters (this battle must be cleared in RSE)";
        private const string evolveWithoutNatDexTooltip = "Allow pokemon to evolve without needing national dex in FRLG";
        private const string mewDeoxysObeyTooltip = "FRLG and Emerald have a special check that makes Mew and Deoxys that aren't recieved from events disobey in battle. This fix removes that mechanic";
        private const string enableEventsTooltip = "Allows mystery gift/event events like Southern Island and Birth Island to be accessed without using the the mystery gift/event systems. You will still need to obtain the relevant item and fulfill all other conditions to unlock the event";

        public CompositeCollection MysteryGiftEventItemDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Mystery gift event items such as the Eon Ticket will not be obtainable, except through normal means or another randomizer setting such as the \"Custom PC Potion\" or \"Custom Shop Item\" settings" },
            new ComboBoxItem() {Content="Start in PC", ToolTip="New save files will start with every relevant mystery gift event item in their PC. WARNING: this only works for new save files. Loading a save state of an old save file will bypass the script that sets the PC starting items" },
            new ComboBoxItem() {Content="Allow in Item Randomization", ToolTip="Mystery gift event items will be able to appear as randomized items even though they are key items. They will be considered to be part of the \"Special\" item category for the purposes of duplicate reduction and all other item randomization settings"},
        };
        public MiscDataView(MiscDataModel model, RomMetadata metadata)
        {
            // Create stack and add content
            var stack = CreateStack();
            Content = stack;

            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            stack.Add(new BoundCheckBoxUI(model.RunIndoors, "Run Indoors"));
            stack.Add(new BoundCheckBoxUI(model.UpdateDOTMoves, "Update Wrap Moves", "Updates the moves Wrap, Bind, Fire Spin, Sand Tomb, Whirlpool, and Clamp to their Gen V power, accuracy, and PP"));
            var enableEventsCb = stack.Add(new BoundCheckBoxUI(model.EnableEvents, "Enable Mystery Gift Events", enableEventsTooltip));
            stack.Add(enableEventsCb.BindEnabled(new EnumComboBoxUI<Settings.MysteryGiftItemSetting>("Mystery Gift Event Item Acquisition", MysteryGiftEventItemDropdown, model.EventItemSetting)));

            stack.Header("Randomizer Options");
            stack.Add(new BoundCheckBoxUI(model.CountRelicanthAsFossil, "Count Relicanth as a Fossil Pokemon"));
            if (metadata.IsRubySapphireOrEmerald)
            {
                stack.Header("Ruby, Sapphire, and Emerald Options");
                stack.Add(new BoundCheckBoxUI(model.RandomizeWallyAce, "Randomize Catching Tutorial Pokemon / Wally Ace"));
                stack.Add(new BoundCheckBoxUI(model.EasyFirstRivalbattle, "Easy First Rival Battle") { ToolTip = easyRivalBattleTooltip });
                var berryRand = stack.Add(new RandomChanceUI("Randomize Berry Trees", model.RandomizeBerryTrees, model.BerryTreeRandomizationChance) { ToolTip = "Randomize which berry trees start in the berry tree slots. WARNING: this only works for new save files. Loading a save state of an old save file will bypass the script that sets the starting berry trees." });
                var berryStack = stack.Add(berryRand.BindEnabled(new StackPanel{ Orientation = Orientation.Horizontal }));
                berryStack.Add(new BoundCheckBoxUI(model.BanEvBerries, "Ban EV Berries") { ToolTip = "Prevent EV-Lowering berries from appearing in randomized berry trees" });
                berryStack.Add(new BoundCheckBoxUI(model.BanMinigameBerries, "Ban Pokéblock / Minigame Berries") { ToolTip = "Prevent Pokéblock / Minigame berries from appearing in randomized berry trees" });
                berryStack.Add(new BoundCheckBoxUI(model.RemapBerries, "Remap Berry Trees Global One-To-One") { ToolTip = "If checked, all berry trees of a certain type will randomize to the same new type" });
            }
            else if (metadata.IsFireRedOrLeafGreen)
            {
                stack.Header("Fire Red and Leaf Green Options");
                stack.Add(new BoundCheckBoxUI(model.EvolveWithoutNationalDex, "Evolve Without National Dex") { ToolTip = evolveWithoutNatDexTooltip });
            }
            if(metadata.IsFireRedOrLeafGreen || metadata.IsEmerald)
            {
                stack.Header("Fire Red, Leaf Green, and Emerald Options");
                stack.Add(new BoundCheckBoxUI(model.DeoxysMewObeyFix, "Fix Mew and Deoxys Obedience Issues") { ToolTip = mewDeoxysObeyTooltip });
            }
        }
    }
}
