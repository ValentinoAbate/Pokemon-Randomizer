using System.Collections;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using EnumTypes;
    using DataStructures;
    using static Settings;
    public class MoveCompatibilityRandomizer
    {
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        public MoveCompatibilityRandomizer(Random rand, IDataTranslator dataT)
        {
            this.rand = rand;
            this.dataT = dataT;
        }

        public void RandomizeCompatibility(BitArray compat, Data data)
        {
            switch (data.setting)
            {
                case MoveCompatOption.AllOn:
                    compat.SetAll(true);
                    break;
                case MoveCompatOption.Random:
                    for (int i = 0; i < compat.Length; ++i)
                    {
                        compat[i] = rand.RollSuccess(data.randomChance);
                    }
                    break;
                case MoveCompatOption.RandomKeepNumber:
                    RandomKeepNumber(compat);
                    break;
                case MoveCompatOption.Intelligent:
                    for (int i = 0; i < compat.Length; ++i)
                    {
                        compat[i] = IntelligentCompat(data.moveList[i], data);
                    }
                    break;
            }
        }

        private void RandomKeepNumber(BitArray arr)
        {
            int compatCount = 0;
            // Find the number of trues
            foreach (bool b in arr)
            {
                if (b)
                {
                    ++compatCount;
                }
            }
            // Wipe the compatibility array
            arr.SetAll(false);
            var choices = Enumerable.Range(0, arr.Length).ToList();
            for (int i = 0; i < compatCount; ++i)
            {
                int choice = rand.Choice(choices);
                arr.Set(choice, true);
                choices.Remove(choice);
            }
        }

        private bool IntelligentCompat(Move move, Data data)
        {
            var moveData = dataT.GetMoveData(move);
            var pokemon = data.pokemon;
            if (pokemon.types.Contains(moveData.type))
                return true;
            if (pokemon.learnSet.Any((l) => l.move == moveData.move))
                return true;
            if (pokemon.eggMoves.Contains(moveData.move))
                return true;
            if (moveData.type == PokemonType.NRM)
                return rand.RollSuccess(data.randomChance);
            return rand.RollSuccess(data.intelligentNoise);
        }

        public class Data
        {
            public MoveCompatOption setting;
            public double randomChance;
            public double intelligentNoise;
            public PokemonBaseStats pokemon;
            public Move[] moveList;

            public Data(MoveCompatOption setting, double randomChance, double intelligentNoise, PokemonBaseStats pokemon, Move[] moveList)
            {
                this.setting = setting;
                this.randomChance = randomChance;
                this.intelligentNoise = intelligentNoise;
                this.pokemon = pokemon;
                this.moveList = moveList;
            }
        }
    }
}