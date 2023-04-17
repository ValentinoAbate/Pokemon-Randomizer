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
            stack.Header("Randomization");
            stack.Add(new RandomChanceUI("Randomize Gift Pokemon", model.RandomizeGiftPokemon, model.GiftPokemonRandChance) { ToolTip = giftPokemonTooltip })
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.GiftSpeciesSettings)));
            stack.Header("Restrictions");
            stack.Add(new BoundCheckBoxUI("Ensure Fossil Revives are Fossil Pokemon", model.EnsureFossilRevivesAreFossilPokemon, fossilReviveTooltip));
            stack.Add(new BoundCheckBoxUI("Ensure Gift Eggs are Baby Pokemon", model.EnsureGiftEggsAreBabyPokemon, babyEggTooltip));
            return CreateTabItem("Gift Pokemon", stack);
        }

        public CompositeCollection StaticLegendaryOptionDropdown => new CompositeCollection()
        {
            new ComboBoxItem() {Content="Randomize", ToolTip="Wild Legendaries will be randomized in the same way as other static wild encounters" },
            new ComboBoxItem() {Content="Skip (Don't Randomize)", ToolTip="Wild Legendaries will not be randomized, even if other static wild encounters are" },
            new ComboBoxItem() {Content="Randomize (Ensure Legendary)", ToolTip="Wild Legendaries will randomize to another Legendary, even if \"Ban Legenaries\" is checked"},
        };

        private TabItem CreateStaticPokemonTab(StaticPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Header("Randomization");
            var staticRand = stack.Add(new RandomChanceUI("Randomize Static Wild Pokemon", model.RandomizeStatics, model.StaticRandChance));
            var optionsStack = stack.Add(staticRand.BindEnabled(CreateStack()));
            optionsStack.Add(new PokemonSettingsUI(model.Settings));
            optionsStack.Add(new EnumComboBoxUI<Settings.LegendaryRandSetting>("Legendary Logic", StaticLegendaryOptionDropdown, model.LegendarySetting));
            optionsStack.Add(new BoundCheckBoxUI("Remap Static Enounters", model.Remap, "Randomizes all instances of a static wild pokemon to the same new pokemon"));
            optionsStack.Add(new BoundCheckBoxUI("Prevent Duplicates", model.Remap, "Prevents the same new pokemon being chosen twice during static wild pokemon randomization"));
            return CreateTabItem("Static Wild Pokemon", stack);
        }

        private static CompositeCollection TradeIVDropdown => new()
        {
            new ComboBoxItem() { Content = "Unchanged" },
            new ComboBoxItem() { Content = "Random", ToolTip = "Pokemon recieved from in-game trades normally have fixed IVs. This option sets them to random values" },
            new ComboBoxItem() { Content = "Maximum", ToolTip = "Ensure all recieved pokemon have max IVs" },
        };

        private TabItem CreateTradePokemonTab(InGameTradesDataModel model)
        {
            // Required Pokemon
            var stack = CreateStack();
            stack.Header("Required Pokemon");
            var giveRand = stack.Add(new RandomChanceUI("Randomize Pokemon Required by Trade", model.RandomizeTradeGive, model.TradePokemonGiveRandChance));
            var giveRandStack = stack.Add(giveRand.BindEnabled(CreateStack()));
            giveRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendariesGive));
            giveRandStack.Add(new BoundCheckBoxUI("Match Power", model.TryMatchPowerGive) { ToolTip ="Make it more likely for the pokemon to randomize to one with a similar Base Stat Total, as in-game trades do not have a fixed level"});

            // Recieved Pokemon
            stack.Header("Recieved Pokemon");
            var recieveRand = stack.Add(new RandomChanceUI("Randomize Pokemon Recieved from Trade", model.RandomizeTradeRecieve, model.TradePokemonRecievedRandChance));
            var recieveRandStack = stack.Add(recieveRand.BindEnabled(CreateStack()));
            recieveRandStack.Add(new BoundCheckBoxUI("Ban Legendaries", model.BanLegendariesRecieve));
            recieveRandStack.Add(new BoundCheckBoxUI("Match Power", model.TryMatchPowerRecieve) { ToolTip = "Make it more likely for the pokemon to randomize to one with a similar Base Stat Total, as in-game trades do not have a fixed level" });

            stack.Add(new EnumComboBoxUI<Settings.TradePokemonIVSetting>("Recieved Pokemon IVs", TradeIVDropdown, model.IVSetting));
            stack.Add(new RandomChanceUI("Randomize Held Items", model.RandomizeHeldItems, model.HeldItemRandChance))
                .BindEnabled(stack.Add(new ItemSettingsUI(model.TradeHeldItemSettings, false)));

            return CreateTabItem("In-Game Trades", stack);
        }
    }
}
