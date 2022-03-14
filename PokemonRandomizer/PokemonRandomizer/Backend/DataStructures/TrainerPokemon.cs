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
        public const int numMoves = 4;
        // Trainer pokemon data types.
        public enum DataType
        {
            Basic,
            SpecialMoves,
            HeldItem,
            SpecialMovesAndHeldItem,
        }
        public bool HasSpecialMoves => dataType == DataType.SpecialMoves || dataType == DataType.SpecialMovesAndHeldItem;
        public DataType dataType;
        public Pokemon species;
        public Item heldItem = Item.None;
        public Move[] moves = new Move[numMoves];
        public int level;
        public int IVLevel;

        public TrainerPokemon()
        {

        }

        public TrainerPokemon(TrainerPokemon other)
        {
            dataType = other.dataType;
            species = other.species;
            heldItem = other.heldItem;
            Array.Copy(other.moves, 0, moves, 0, numMoves);
            level = other.level;
            IVLevel = other.IVLevel;
        }

        public override string ToString()
        {
            return species.ToDisplayString() + " Lv. " + level;
        }
    }
}
