using System;
namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainer : Trainer
    {
        private static readonly TrainerClass stevenClass = new TrainerClass()
        {
            Name = "[PK][MN] TRAINER"
        };
        public const string specialTrainerKey = "stevenAllyBattle";
        public override int MaxPokemon => multiBattlePartySize;
        public override string Name { get => "STEVEN"; set { } }
        public override bool IsDoubleBattle { get => false; set { } }
        public override TrainerClass Class { get => stevenClass; set { } }

        public override string ToString()
        {
            return base.ToString() + " (Ally)";
        }
    }
}
