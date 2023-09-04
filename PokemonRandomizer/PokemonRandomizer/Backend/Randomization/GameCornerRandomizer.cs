using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    public class GameCornerRandomizer
    {
        public const int maxBaseWager = 96;
        public const int maxRandomBaseWager = 16;
        private const int maxRandomBaseWagerPlusOne = maxRandomBaseWager + 1;
        private const int maxWager = byte.MaxValue;
        private readonly Random rand;
        public GameCornerRandomizer(Random rand)
        {
            this.rand = rand;
        }

        public enum RouletteRandomizationOption
        {
            None,
            FixedBase,
            RandomBase,
        }

        public void Randomize(RomData data, Settings s)
        {
            RandomizeRoulette(data, s);
        }

        private void RandomizeRoulette(RomData data, Settings s)
        {
            // Modify Roulette Table Wagers (if applicable)
            // The wagers are as follows: [leftTableNormal,rightTableNormal,leftTableSpecial,rightTableSpecial]
            if (s.RouletteWagerOption == RouletteRandomizationOption.None || data.RouletteWagers.Length <= 0)
                return;
            byte baseWager = (byte)(s.RouletteWagerOption switch
            {
                RouletteRandomizationOption.FixedBase => s.FixedBaseRouletteWager,
                RouletteRandomizationOption.RandomBase => rand.RandomInt(2, maxRandomBaseWagerPlusOne),
                _ => 1,
            });
            data.RouletteWagers[0] = data.RouletteWagers[2] = baseWager;
            data.RouletteWagers[1] = (byte)Math.Min(baseWager * 3, maxWager);
            data.RouletteWagers[3] = (byte)Math.Min(baseWager * 6, maxWager);
        }
    }
}
