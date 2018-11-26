using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{

    public class TrainerPokemon
    {
        // Trainer pokemon data types.
        public enum DataType
        {
            Basic,
            SpecialMoves,
            HeldItem,
            SpecialMovesAndHeldItem,
        }
        public DataType dataType;
        public PokemonSpecies species;
        public Item heldItem = Item.None;
        public Move[] moves = new Move[4];
        public int level;
        public BitArray AIFlags = new BitArray(32);
    }
}
