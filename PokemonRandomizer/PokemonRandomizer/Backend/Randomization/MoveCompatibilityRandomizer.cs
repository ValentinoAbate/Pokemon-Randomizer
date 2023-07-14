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

        public void RandomizeCompatibility(MoveCompatOption setting, BitArray compat, Move[] moveList, PokemonBaseStats pokemon, Data data)
        {
            switch (setting)
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
                    // If the pokemon is intentionally designed to learn all TM/HM/MT moves (e.g Mew), set all true
                    if (pokemon.originalUnlearnableTmHmMtMoves.Count == 0)
                    {
                        compat.SetAll(true);
                        break;
                    }
                    for (int i = 0; i < compat.Length; ++i)
                    {
                        compat[i] = IntelligentCompat(moveList[i], pokemon, data);
                    }
                    break;
            }
        }

        public void RandomizeHMCompat(MoveCompatOption setting, BitArray compat, Move[] moveList, PokemonBaseStats pokemon)
        {
            if(setting == MoveCompatOption.AllOn)
            {
                compat.SetAll(true);
            }
            else if(setting == MoveCompatOption.Intelligent && pokemon.IsVariant)
            {
                var variantTypes = pokemon.VariantTypes;
                for (int i = 0; i < compat.Length; ++i)
                {
                    if (compat[i])
                        continue;
                    // Make the pokemon compatible with any HMs that are the same type as one of it's variant types
                    var moveData = dataT.GetMoveData(moveList[i]);
                    foreach (var type in variantTypes)
                    {
                        if (moveData.IsType(type) || SpecialHMCompat(moveData.move, type))
                        {
                            compat[i] = true;
                        }
                    }
                }
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

        private bool IntelligentCompat(Move move, PokemonBaseStats pokemon, Data data)
        {
            var moveData = dataT.GetMoveData(move);
            if (pokemon.learnSet.Any(l => l.move == move))
                return true;
            // If the pokemon is intentionally designed to not learn TM/HM/MT moves (e.g Magikarp, etc.), set all false (except moves in the moveset)
            if (pokemon.originalTmHmMtMoves.Count == 0)
                return false;
            if (pokemon.originalTmHmMtMoves.Contains(move))
                return true;
            if (pokemon.VariantTypes.Contains(moveData.type))
                return true;
            if (pokemon.species is Pokemon.CASTFORM && CastformCompat(moveData))
                return true;
            if (pokemon.eggMoves.Contains(move))
                return true;
            if (pokemon.originalUnlearnableTmHmMtMoves.Contains(move))
                return false;
            if (moveData.IsType(pokemon))
                return true;
            if (moveData.IsType(PokemonType.NRM))
                return rand.RollSuccess(data.intelligentNormalRandChance);
            return rand.RollSuccess(data.intelligentRandChance);
        }

        // Allow castform access to all fire / water / ice / electric moves
        // Maybe require moves to be special
        private bool CastformCompat(MoveData moveData)
        {
            return !moveData.IsStatus && (moveData.IsType(PokemonType.FIR) || moveData.IsType(PokemonType.WAT) || moveData.IsType(PokemonType.ICE) || moveData.IsType(PokemonType.ELE));
        }

        // Special variant HM compatibility for off-type attacking moves (can't be handled by type overrides)
        private bool SpecialHMCompat(Move move, PokemonType type)
        {
            if(move is Move.STRENGTH)
            {
                // Fighting types always get access to strength
                return type is PokemonType.FTG;
            }
            if(move is Move.CUT)
            {
                // Bug types always get cut
                return type is PokemonType.BUG;
            }
            if(move is Move.ROCK_CLIMB)
            {
                // Rock and fighting types always get rock climb
                return type is PokemonType.RCK or PokemonType.FTG;
            }
            return false;
        }

        public class Data
        {
            public double randomChance;
            public double intelligentNormalRandChance;
            public double intelligentRandChance;

            public Data(double randomChance, double intelligentNormalRandChance, double intelligentRandChance)
            {
                this.randomChance = randomChance;
                this.intelligentNormalRandChance = intelligentNormalRandChance;
                this.intelligentRandChance = intelligentRandChance;
            }
        }
    }
}