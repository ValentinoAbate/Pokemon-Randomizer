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
        public Box<bool> BanLegendariesGive { get; set; } = new Box<bool>(true);
        public Box<bool> TryMatchPowerGive { get; set; } = new Box<bool>(true);
        public Box<bool> RandomizeTradeRecieve { get; set; } = new Box<bool>();
        public Box<double> TradePokemonRecievedRandChance { get; set; } = new Box<double>(1);
        public Box<bool> BanLegendariesRecieve { get; set; } = new Box<bool>(true);
        public Box<bool> TryMatchPowerRecieve { get; set; } = new Box<bool>(true);
        public Box<TradePokemonIVSetting> IVSetting { get; set; } = new Box<TradePokemonIVSetting>(TradePokemonIVSetting.Unchanged);
        public Box<bool> RandomizeHeldItems { get; set; } = new Box<bool>();
        public Box<double> HeldItemRandChance { get; set; } = new Box<double>(1);
        public ItemRandomizer.Settings TradeHeldItemSettings { get; set; } = new ItemRandomizer.Settings()
        {
            NoneToOtherChance = 1,
        };
    }
}
