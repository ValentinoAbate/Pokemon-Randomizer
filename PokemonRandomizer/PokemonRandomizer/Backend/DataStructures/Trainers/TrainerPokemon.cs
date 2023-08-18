using System;
using System.Collections;
using System.Collections.Generic;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class TrainerPokemon : ITrainerPokemon
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

        #region ITrainerPokemon implementation
        public IReadOnlyList<Move> Moves => moves;
        public Pokemon Species => species;
        #endregion

        public bool HasSpecialMoves => dataType == DataType.SpecialMoves || dataType == DataType.SpecialMovesAndHeldItem;
        public DataType dataType;
        public Pokemon species;
        public Item heldItem = Item.None;
        public Move[] moves = new Move[numMoves];
        public int level;
        public int IVLevel;

        public virtual int MaxIV => byte.MaxValue;

        public virtual TrainerPokemon Clone()
        {
            var other = new TrainerPokemon();
            other.CopyBasicValuesFrom(this);
            return other;
        }

        protected void CopyBasicValuesFrom(TrainerPokemon other)
        {
            dataType = other.dataType;
            species = other.species;
            heldItem = other.heldItem;
            Array.Copy(other.moves, moves, numMoves);
            level = other.level;
            IVLevel = other.IVLevel;
        }

        public override string ToString()
        {
            return species.ToDisplayString() + " Lv. " + level;
        }
    }
}
