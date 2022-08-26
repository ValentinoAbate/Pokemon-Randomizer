using System;
namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainer : Trainer
    {
        public const string specialTrainerKey = "stevenAllyBattle";
        public override string Name { get => "STEVEN"; set { } }
        public override bool IsDoubleBattle { get => false; set { } }
        public override string Class => "[PK][MN] TRAINER";

        public override string ToString()
        {
            return base.ToString() + " (Ally)";
        }
    }
}
