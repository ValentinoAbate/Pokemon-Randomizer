using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.Utilities.Repointing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public abstract class Trainer
    {
        public enum Category
        {
            Trainer,
            AceTrainer,
            Rival,
            TeamGrunt,
            TeamAdmin,
            TeamLeader,
            GymLeader,
            GymTrainer,
            EliteFour,
            Champion,
            CatchingTutTrainer, // Wally, Etc
            SpecialBoss, // Post-game steven in emerald, Red, etc
        }

        public const int multiBattlePartySize = 3;
        public const int maxPartySize = 6;
        public const string nullName = "??????";
        public const int nameLength = 12;
        public bool Invalid => string.IsNullOrWhiteSpace(Name) || Name == nullName;
        public virtual int MaxPokemon => maxPartySize;
        public static int AverageLevelComparer(Trainer t1, Trainer t2) => t1.AvgLvl.CompareTo(t2.AvgLvl);

        public double AvgLvl => Pokemon.Count > 0 ? Pokemon.Average((p) => p.level) : 0;

        public Category TrainerCategory { get; set; }
        public TrainerThemeData ThemeData { get; set; }

        public int offset;
        public abstract string Class { get; }
        public int trainerClass;
        public abstract string Name { get; set; }
        public abstract bool IsDoubleBattle { get; set; }

        public int pokemonOffset; // move to pokemon data
        public List<TrainerPokemon> Pokemon => PokemonData.Pokemon;
        public TrainerPokemon.DataType DataType
        {
            get => PokemonData.DataType;
            set => PokemonData.DataType = value;
        }
        public TrainerPokemonData PokemonData { get; set; }

        public void EnsureSafeBattleType()
        {
            if (Pokemon.Count <= 1 && IsDoubleBattle)
            {
                IsDoubleBattle = false;
            }
        }

        public override string ToString()
        {
            return $"{Class} {Name} ({TrainerCategory})";
        }

        public class TrainerPokemonData : IRepointable
        {
            public bool NeedsRepoint => DataType != originalDataType || Pokemon.Count > originalPokemonCount;
            public TrainerPokemon.DataType DataType
            {
                get => dataType;
                set
                {
                    if (dataType == value)
                        return;
                    dataType = value;
                    foreach (var pokemon in Pokemon)
                    {
                        pokemon.dataType = value;
                    }
                }
            }
            private TrainerPokemon.DataType dataType;
            private readonly TrainerPokemon.DataType originalDataType;
            public List<TrainerPokemon> Pokemon { get; }
            private readonly int originalPokemonCount;
            public TrainerPokemonData(TrainerPokemon.DataType dataType, int numPokemon)
            {
                Pokemon = new List<TrainerPokemon>(numPokemon);
                originalPokemonCount = numPokemon;
                this.dataType = dataType;
                originalDataType = dataType;
            }
        }
    }
}
