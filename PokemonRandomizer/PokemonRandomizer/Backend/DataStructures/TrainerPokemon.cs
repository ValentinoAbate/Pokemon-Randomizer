using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
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
        public bool HasSpecialMoves { get => dataType == DataType.SpecialMoves || dataType == DataType.SpecialMovesAndHeldItem; }
        public DataType dataType;
        public PokemonSpecies species;
        public Item heldItem = Item.None;
        public Move[] moves = new Move[4];
        public int level;
        public int IVLevel;

        public override string ToString()
        {
            return species.ToDisplayString() + " Lv. " + level;
        }
    }
}
