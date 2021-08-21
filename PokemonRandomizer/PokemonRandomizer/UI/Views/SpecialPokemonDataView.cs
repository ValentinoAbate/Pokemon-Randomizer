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
            tabs.Items.Add(CreateTabItem("Starters", new StartersDataView(model.StarterData, pokemonNames, pokemon)));
            // Gift pokemon
            // Trade pokemon
            tabs.Items.Add(CreateTradePokemonTab(model.TradeData));

        }

        private TabItem CreateTradePokemonTab(InGameTradesDataModel model)
        {
            var tabs = new TabControl();
            // Required Pokemon Tab
            var stack = CreateStack();
            Header("Randomization", stack);
            stack.Add(new RandomChanceUI("Randomize Pokemon Required by Trade", model.RandomizeTradeGive, model.TradePokemonGiveRandChance))
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.TradeSpeciesSettingsGive)));
            tabs.Items.Add(CreateTabItem("Required Pokemon", stack));
            // Recieved Pokemon Tab
            stack = CreateStack();
            Header("Randomization", stack);
            stack.Add(new RandomChanceUI("Randomize Pokemon Recieved by Trade", model.RandomizeTradeRecieve, model.TradePokemonRecievedRandChance))
                .BindEnabled(stack.Add(new PokemonSettingsUI(model.TradeSpeciesSettingsRecieve)));
            tabs.Items.Add(CreateTabItem("Recieved Pokemon", stack));
            // Held Item Tab
            stack = CreateStack();
            Header("Randomization", stack);
            stack.Add(new RandomChanceUI("Randomize Held Items", model.RandomizeHeldItems, model.HeldItemRandChance))
                .BindEnabled(stack.Add(new ItemSettingsUI(model.TradeHeldItemSettings)));
            tabs.Items.Add(CreateTabItem("Held Items", stack));

            return CreateTabItem("In-Game Trades", tabs);
        }
    }
}
