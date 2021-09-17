using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.UI.Utilities;
using System.Collections.Generic;
using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class InGameTradesDataModel : DataModel
    {
        public Box<bool> RandomizeTradeGive { get; set; } = new Box<bool>();
        public Box<double> TradePokemonGiveRandChance { get; set; } = new Box<double>(1);
        public Box<bool> RandomizeTradeRecieve { get; set; } = new Box<bool>();
        public Box<double> TradePokemonRecievedRandChance { get; set; } = new Box<double>(1);
        public PokemonSettings TradeSpeciesSettingsGive { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.powerIndividual)
            }
        };
        public PokemonSettings TradeSpeciesSettingsRecieve { get; set; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.powerIndividual)
            }
        };
        public Box<bool> RandomizeHeldItems { get; set; } = new Box<bool>();
        public Box<double> HeldItemRandChance { get; set; } = new Box<double>(1);
        public ItemRandomizer.Settings TradeHeldItemSettings { get; set; } = new ItemRandomizer.Settings()
        {
            SamePocketChance = 0,
            NoneToOtherChance = 1,
        };
    }
}
