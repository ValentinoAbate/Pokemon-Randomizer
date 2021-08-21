using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class InGameTradesDataModel
    {
        public Box<bool> RandomizeTradeGive { get; } = new Box<bool>();
        public Box<double> TradePokemonGiveRandChance { get; } = new Box<double>(1);
        public Box<bool> RandomizeTradeRecieve { get; } = new Box<bool>();
        public Box<double> TradePokemonRecievedRandChance { get; } = new Box<double>(1);
        public PokemonSettings TradeSpeciesSettingsGive { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.powerIndividual)
            }
        };
        public PokemonSettings TradeSpeciesSettingsRecieve { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.powerIndividual)
            }
        };
        public Box<bool> RandomizeHeldItems { get; } = new Box<bool>();
        public Box<double> HeldItemRandChance { get; } = new Box<double>(1);
        public ItemRandomizer.Settings TradeHeldItemSettings { get; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0,
            NoneToOtherChance = 1,
        };
    }
}
