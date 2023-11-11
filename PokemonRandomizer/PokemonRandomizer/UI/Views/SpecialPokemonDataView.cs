using System.Collections.Generic;
using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;
    using System.Windows.Data;

    public class SpecialPokemonDataView : DataView<SpecialPokemonDataModel>
    {
        public SpecialPokemonDataView(SpecialPokemonDataModel model, string[] pokemonNames, List<Pokemon> pokemon)
        {
            var tabs = CreateMainTabControl();

            // Starters
            tabs.Add(CreateTabItem("Starter Pokemon", new StartersDataView(model.StarterData, pokemonNames, pokemon)));
            // Dream Team
            tabs.Add(CreateTabItem("Dream Team", new DreamTeamDataView(model.DreamTeamData, pokemonNames, pokemon)));
            // Gift pokemon
            tabs.Add(CreateGiftPokemonTab(model.GiftData));
            // Static Pokemon
            tabs.Add(CreateStaticPokemonTab(model.StaticPokemonData));
            // Trade pokemon
            tabs.Add(CreateTradePokemonTab(model.TradeData));

        }

        private const string giftPokemonTooltip = "Randomizes pokemon and pokemon eggs given by NPCs (such as the Castform given at the weather institute)" +
            "\nAlso randomizes pokemon given by pokeballs and other such methods (such as the Eevee in Celadon City)";
        private const string fossilReviveTooltip = "Ensures that randomized pokemon that are revived from fossils will be fossil pokemon";
        private const string babyEggTooltip = "Ensures that randomized gift eggs will hatch into baby pokemon";

        private TabItem CreateGiftPokemonTab(GiftPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Header("Gift Pokemon Randomization");
            stack.Add(new RandomChanceUI("Randomize Gift Pokemon", model.RandomizeGiftPokemon, model.GiftPokemonRandChance) { ToolTip = giftPokemonTooltip })
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.GiftSpeciesSettings)));
            stack.Header("Restrictions");
            stack.Add(new BoundCheckBoxUI("Ensure Fossil Revives are Fossil Pokemon", model.EnsureFossilRevivesAreFossilPokemon, fossilReviveTooltip));
            stack.Add(new BoundCheckBoxUI("Ensure Gift Eggs are Baby Pokemon", model.EnsureGiftEggsAreBabyPokemon, babyEggTooltip));
            return CreateTabItem("Gift Pokemon", stack);
        }

        public CompositeCollection StaticLegendaryOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Randomize", ToolTip="Static legendaries will be randomized in the same way as other static wild encounters" },
            new ComboBoxItem() {Content="Skip (Don't Randomize)", ToolTip="Static legendaries will not be randomized, even if other static wild encounters are" },
            new ComboBoxItem() {Content="Randomize (Ensure Legendary)", ToolTip="Static legendaries will randomize to another legendary, even if \"Ban Legendaries\" is checked"},
        };

        private const string staticWildTooltip = "Randomizes the pokemon encountered in static wild encounters, such as Voltorb pokeballs and legendary encounters";

        private TabItem CreateStaticPokemonTab(StaticPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Header("Static Pokemon Randomization");
            var staticRand = stack.Add(new RandomChanceUI("Randomize Static Wild Pokemon", model.RandomizeStatics, model.StaticRandChance) { ToolTip = staticWildTooltip });
            var optionsStack = stack.Add(staticRand.BindEnabled(CreateStack()));
            optionsStack.Add(new PokemonSettingsUI(model.Settings));
            optionsStack.Add(new EnumComboBoxUI<Settings.LegendaryRandSetting>("Legendary Logic", StaticLegendaryOptionDropdown, model.LegendarySetting) { ToolTip = "The logic used when randomizing static legendary pokemon" });
            optionsStack.Add(new BoundCheckBoxUI("Remap Static Enounters", model.Remap, "Randomizes all instances of a static wild pokemon to the same new pokemon"));
            optionsStack.Add(new BoundCheckBoxUI("Prevent Duplicates", model.Remap, "Prevents the same new pokemon being chosen twice during static wild pokemon randomization"));
            return CreateTabItem("Static Wild Pokemon", stack);
        }

        private static CompositeCollection TradeIVDropdown => new()
        {
            new ComboBoxItem() { Content = "Unchanged", ToolTip="Pokemon recieved from in-game trades will have the same fixed IVs they do in the base game" },
            new ComboBoxItem() { Content = "Random", ToolTip = "Pokemon recieved from in-game trades will have random IVs, instead of the fixed IVs from the base game" },
            new ComboBoxItem() { Content = "Maximum", ToolTip = "Pokemon recieved from in-game trades will have max IVs" },
        };

        private const string tradeRequestedPokemonTooltip = "Randomizes the pokemon that in-game trades request from the player";
        private const string tradeRequestedBanLegendsTooltip = "Ensure that in-game trades never request a legendary pokemon";
        private const string tradeOfferedPokemonTooltip = "Randomizes the pokemon that in-game trades offer to the player";
        private const string tradeOfferedBanLegendsTooltip = "Ensure that in-game trades never offer a legendary pokemon";
        private const string tradeMatchPowerTooltip = "Make it more likely for the pokemon to randomize to one with a similar Base Stat Total, as in-game trades do not have a fixed level";
        private const string tradeIVDropdownTooltip = "The logic used to modify the fixed IVs of the pokemon recieved from in-game trades";
        private const string tradeHeldItemRandTooltip = "Randomizes the items held by the pokemon recieved from in-game trades" +
            "\nIf this option is on, all pokemon recieved from in-game trades will have held items (even those that don't in the base game)";

        private TabItem CreateTradePokemonTab(InGameTradesDataModel model)
        {
            // Required Pokemon
            var stack = CreateStack();
            stack.Header("Requested Pokemon");
            var giveRand = stack.Add(new RandomChanceUI("Randomize Pokemon Requested by Trade", model.RandomizeTradeGive, model.TradePokemonGiveRandChance) { ToolTip = tradeRequestedPokemonTooltip });
            var giveRandStack = stack.Add(giveRand.BindEnabled(CreateStack()));
            giveRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendariesGive, tradeRequestedBanLegendsTooltip));
            giveRandStack.Add(new BoundCheckBoxUI("Match Power", model.TryMatchPowerGive, tradeMatchPowerTooltip));

            // Recieved Pokemon
            stack.Header("Offered Pokemon");
            var recieveRand = stack.Add(new RandomChanceUI("Randomize Pokemon Offered for Trade", model.RandomizeTradeRecieve, model.TradePokemonRecievedRandChance) { ToolTip = tradeOfferedPokemonTooltip });
            var recieveRandStack = stack.Add(recieveRand.BindEnabled(CreateStack()));
            recieveRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendariesRecieve, tradeOfferedBanLegendsTooltip));
            recieveRandStack.Add(new BoundCheckBoxUI("Match Power", model.TryMatchPowerRecieve, tradeMatchPowerTooltip));

            stack.Add(new EnumComboBoxUI<Settings.TradePokemonIVSetting>("Recieved Pokemon IVs", TradeIVDropdown, model.IVSetting) { ToolTip = tradeIVDropdownTooltip });
            stack.Add(new RandomChanceUI("Randomize Held Items", model.RandomizeHeldItems, model.HeldItemRandChance) { ToolTip = tradeHeldItemRandTooltip })
                .BindEnabled(stack.Add(new ItemSettingsUI(model.TradeHeldItemSettings, false)));

            return CreateTabItem("In-Game Trades", stack);
        }
    }
}
