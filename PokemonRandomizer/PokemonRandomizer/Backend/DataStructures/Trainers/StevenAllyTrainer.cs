using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public class StevenAllyTrainer : Trainer
    {
        public override string Name { get => "STEVEN"; set { } }
        public override bool IsDoubleBattle { get => false; set { } }
        public override string Class => "[Pk][mn] trainer";
    }
}
