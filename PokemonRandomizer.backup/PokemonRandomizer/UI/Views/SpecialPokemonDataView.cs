using System.Collections.Generic;
using System.Windows.Controls;

namespace PokemonRandomizer.UI.Views
{
    using Backend.EnumTypes;
    using Models;


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
            // Trade pokemon
            tabs.Add(CreateTradePokemonTab(model.TradeData));

        }

        private TabItem CreateGiftPokemonTab(GiftPokemonDataModel model)
        {
            var stack = CreateStack();
            stack.Header("Randomization");
            stack.Add(new RandomChanceUI("Randomize Gift Pokemon", model.RandomizeGiftPokemon, model.GiftPokemonRandChance))
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.GiftSpeciesSettings)));
            stack.Header("Restrictions");
            stack.Add(new BoundCheckBoxUI(model.EnsureFossilRevivesAreFossilPokemon, "Ensure Fossil Revives are Fossil Pokemon"));
            stack.Add(new BoundCheckBoxUI(model.EnsureGiftEggsAreBabyPokemon, "Ensure Gift Eggs are Baby Pokemon"));
            return CreateTabItem("Gift Pokemon", stack);
        }

        private TabItem CreateTradePokemonTab(InGameTradesDataModel model)
        {
            var tabs = new TabControl();
            // Required Pokemon Tab
            var stack = CreateStack();
            stack.Header("Randomization");
            stack.Add(new RandomChanceUI("Randomize Pokemon Required by Trade", model.RandomizeTradeGive, model.TradePokemonGiveRandChance))
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.TradeSpeciesSettingsGive)));
            tabs.Add(CreateTabItem("Required Pokemon", stack));
            // Recieved Pokemon Tab
            stack = CreateStack();
            stack.Header("Randomization");
            stack.Add(new RandomChanceUI("Randomize Pokemon Received by Trade", model.RandomizeTradeRecieve, model.TradePokemonRecievedRandChance))
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.TradeSpeciesSettingsRecieve)));
            tabs.Add(CreateTabItem("Received Pokemon", stack));
            // Held Item Tab
            stack = CreateStack();
            stack.Header("Randomization");
            stack.Add(new RandomChanceUI("Randomize Held Items", model.RandomizeHeldItems, model.HeldItemRandChance))
                .BindEnabled(stack.Add(new ItemSettingsUI(model.TradeHeldItemSettings)));
            tabs.Add(CreateTabItem("Held Items", stack));

            return CreateTabItem("In-Game Trades", tabs);
        }
    }
}
