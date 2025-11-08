using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Models;
    using Backend.DataStructures;
    using System.Windows.Data;

    public class MiscDataView : DataView<MiscDataModel>
    {
        private const string easyRivalBattleTooltip = "Sets the first rival battle to have level 1 pokemon. Useful for randomized starters (this battle must be cleared in RSE)";
        private const string evolveWithoutNatDexTooltip = "Allows all pokemon to evolve without needing the national dex in FRLG";
        private const string mewDeoxysObeyTooltip = "FRLG and Emerald have a special check that makes Mew and Deoxys that aren't recieved from events disobey in battle. This fix removes that mechanic";
        private const string enableEventsTooltip = "Allows mystery gift/event events like Southern Island and Birth Island to be accessed without using the the mystery gift/event systems. You will still need to obtain the relevant item and fulfill all other conditions to unlock the event";
        private const string upgradeUnownTooltip = "Give Unown access to every \"Power\" move available (Hidden Power, Nature Power, Secret Power, Cosmic Power, Ancientpower, Superpower, etc.)";
        private const string upgradeCastformTooltip = "Give Castform earlier access to Weather Ball, and give it access to Sandstorm, Thunder, Blizzard, and Solarbeam by level-up" +
            "\nThis allows Castform to retain access to important weather-related moves even when TMs are randomized and use these moves in NPC movesets";
        private const string updateRotomAppliancesTooltip = "Updates Rotom so its secondary type changes with its appliance form as it does in Gen V+";
        private const string addWeatherAbilitiesTooltup = "Distribute Drizzle and Drought to the pokemon that can have them as of Gen VII+" +
            "\nPelliper gains Drizzle as a secondary ability, and Wingull gets Rain Dish as a secondary ability (so that you can tell what ability it will have when evolved)" +
            "\nPolitoed gains Drizzle as a secondary ability, replacing Damp" +
            "\nVulpix, Ninetales, and Torkoal all gain Drought as a secondary ability" +
            "\nNote: link battles between a ROM that has \"Distribute Weather Abilities\" applied and one that doesn't that include affected pokemon may behave incorrectly";
        private const string typeEffectivenessModLinkWarning = "\nNote: link battles between two ROMs that have different \"Type Effectiveness Modification\" settings may behave incorrectly";
        private const string updateDOTMovesTooltip = "Updates the moves Wrap, Bind, Fire Spin, Sand Tomb, Whirlpool, Clamp, and Magma Storm to their Gen V power, accuracy, and PP" +
            "\nNote: link battles between a ROM that has \"Update Wrap Moves\" applied and one that doesn't may behave incorrectly when using affected moves";
        private const string fixStevenNaturesTooltip = "Fixes a bug in the Emerald base game that causes Steven's pokemon in the Steven ally battle to not have the correct natures";
        private const string startWithNationalDexTooltip = "Start the game with the national pokedex instead of the regional pokedex" +
            "\nAllows you to check catching areas of national dex pokemon, and allows trading national dex pokemon in Emerald";

        private const string relicanthFossilTooltip = "Counts Relicanth as a fossil pokemon for the purposes of randomizer seetings such as the \"Ensure Fossil Revives are Fossil Pokemon\" setting";
        private const string runIndoorsTooltip = "Allows the player to run indoors once the running shoes have been obtained" +
            "\nRunning is still disabled in certain special areas like the Fortree walkways and Pacifidlog town";
        private const string mysteryGiftItemDistributionTooltip = "Controls how mystery gift event items such as the Eon Ticket appear in the game";
        private const string catchingTutWallyAceTooltip = "Randomizes the catching tutorial pokemon and ensures that it becomes Wally's ace pokemon for his first battle" +
            "\nWhether or not Wally will keep his ace pokemon for subsequent battles is controlled by the \"Recurring Trainer Pokemon Randomization Strategy\" setting in the \"Trainers\" tab";
        private const string typeChartModTooltip = "Applys a modification to the game's type relation chart. The updated type chart will be printed in your ROM's info file for reference";
        private static CompositeCollection MysteryGiftEventItemDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Mystery gift event items such as the Eon Ticket will not be obtainable, except through normal means or another randomizer setting such as the \"Custom PC Potion\" or \"Custom Shop Item\" settings" },
            new ComboBoxItem() {Content="Start in PC", ToolTip="New save files will start with every relevant mystery gift event item in their PC. WARNING: this only works for new save files. Loading a save state of an old save file will bypass the script that sets the PC starting items" },
            new ComboBoxItem() {Content="Allow in Item Randomization", ToolTip="Mystery gift event items will be able to appear as randomized items even though they are key items. They will be considered to be part of the \"Special\" item category for the purposes of duplicate reduction and all other item randomization settings"},
        };

        private static CompositeCollection TypeChartOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="None", ToolTip="Type effectiveness will be left as it is in the base game" },
            new ComboBoxItem() {Content="Invert", ToolTip="Type effectiveness will be inverted (like in inverse battles). Any weakness will become a resistance, and any resistance or immunity will become a weakness" + typeEffectivenessModLinkWarning},
            new ComboBoxItem() {Content="Swap", ToolTip="Type effectiveness will be swapped. For example, (DRG is weak to ICE) becomes (ICE is weak to DRG), (GHO is immune to FTG) becomes (FTG is immune to GHO), etc." + typeEffectivenessModLinkWarning},
        };

        public MiscDataView(MiscDataModel model, RomMetadata metadata)
        {
            // Create stack and add content
            var stack = CreateMainStack();

            stack.Header(UISkin.Current.HacksAndTweaksHeader);
            var basicTweaksStack = stack.Add(CreateHorizontalStack());
            if (metadata.IsGenIII)
            {
                basicTweaksStack.Add(new BoundCheckBoxUI("Run Indoors", model.RunIndoors, runIndoorsTooltip));
            }
            if (metadata.IsRubySapphireOrEmerald)
            {
                basicTweaksStack.Add(new BoundCheckBoxUI("Start With National Dex", model.StartWithNationalDex, startWithNationalDexTooltip));
            }
            stack.Add(new BoundCheckBoxUI("Update Wrap Moves", model.UpdateDOTMoves, updateDOTMovesTooltip));
            var pokemonTweakStack = stack.Add(CreateHorizontalStack());
            pokemonTweakStack.Add(new BoundCheckBoxUI("Upgrade Unown", model.UpgradeUnown, upgradeUnownTooltip));
            pokemonTweakStack.Add(new BoundCheckBoxUI("Upgrade Castform", model.UpgradeCastform, upgradeCastformTooltip));
            if(metadata.IsPlatinum || metadata.IsHGSS)
            {
                pokemonTweakStack.Add(new BoundCheckBoxUI("Update Rotom Appliance Types", model.UpdateRotomApplianceTypes, updateRotomAppliancesTooltip));
            }
            stack.Add(new BoundCheckBoxUI("Distribute Weather Abilities", model.AddWeatherAbilities, addWeatherAbilitiesTooltup));
            var enableEventsCb = stack.Add(new BoundCheckBoxUI("Enable Mystery Gift Events", model.EnableEvents, enableEventsTooltip));
            stack.Add(enableEventsCb.BindEnabled(new EnumComboBoxUI<Settings.MysteryGiftItemSetting>("Mystery Gift Event Item Acquisition", MysteryGiftEventItemDropdown, model.EventItemSetting) { ToolTip = mysteryGiftItemDistributionTooltip } ));
            stack.Add(new EnumComboBoxUI<Backend.Randomization.TypeChartRandomizer.Option>("Type Effectiveness Modification", TypeChartOptionDropdown, model.TypeChartSetting) { ToolTip = typeChartModTooltip });

            stack.Header("Randomizer Options");
            stack.Add(new BoundCheckBoxUI("Count Relicanth as a Fossil Pokemon", model.CountRelicanthAsFossil, relicanthFossilTooltip));
            if (metadata.IsRubySapphireOrEmerald)
            {
                stack.Header("Ruby, Sapphire, and Emerald Options");
                stack.Add(new BoundCheckBoxUI("Randomize Catching Tutorial Pokemon / Wally Ace", model.RandomizeWallyAce, catchingTutWallyAceTooltip));
                stack.Add(new BoundCheckBoxUI("Easy First Rival Battle", model.EasyFirstRivalbattle, easyRivalBattleTooltip));
                var berryRand = stack.Add(new RandomChanceUI("Randomize Berry Trees", model.RandomizeBerryTrees, model.BerryTreeRandomizationChance) { ToolTip = "Randomize which berry trees start in the berry tree slots. WARNING: this only works for new save files. Loading a save state of an old save file will bypass the script that sets the starting berry trees." });
                var berryStack = stack.Add(berryRand.BindEnabled(CreateHorizontalStack()));
                berryStack.Add(new BoundCheckBoxUI("Ban EV Berries", model.BanEvBerries, "Prevent EV-Lowering berries from appearing in randomized berry trees"));
                berryStack.Add(new BoundCheckBoxUI("Ban Pokéblock / Minigame Berries", model.BanMinigameBerries, "Prevent Pokéblock / Minigame berries from appearing in randomized berry trees"));
                berryStack.Add(new BoundCheckBoxUI("Remap Berry Trees Global One-To-One", model.RemapBerries, "If checked, all berry trees of a certain type will randomize to the same new type"));
            }
            else if (metadata.IsFireRedOrLeafGreen)
            {
                stack.Header("Fire Red and Leaf Green Options");
                stack.Add(new BoundCheckBoxUI("Evolve Without National Dex", model.EvolveWithoutNationalDex, evolveWithoutNatDexTooltip));
            }
            if (metadata.IsEmerald)
            {
                stack.Header("Emerald Options");
                stack.Add(new BoundCheckBoxUI("Fix Steven Ally Battle Natures", model.FixStevenNatures, fixStevenNaturesTooltip));
            }
            if (metadata.IsFireRedOrLeafGreen || metadata.IsEmerald)
            {
                stack.Header("Fire Red, Leaf Green, and Emerald Options");
                stack.Add(new BoundCheckBoxUI("Fix Mew and Deoxys Obedience Issues", model.DeoxysMewObeyFix, mewDeoxysObeyTooltip));
            }
        }
    }
}
