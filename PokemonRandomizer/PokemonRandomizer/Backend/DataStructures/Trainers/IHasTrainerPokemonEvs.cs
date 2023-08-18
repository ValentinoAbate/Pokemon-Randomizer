using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Trainers
{
    public interface IHasTrainerPokemonEvs
    {
        public const byte maxUsefulEvValue = 252;
        public const int maxEvs = 510;
        public const int leftoverEvs = maxEvs - (maxUsefulEvValue * 2);
        public const int totalEvStatPoints = maxEvs / evsPerStatPoint;
        public const int maxUsefulEvStatPointValue = maxUsefulEvValue / evsPerStatPoint;
        public const int evsPerStatPoint = 4;
        public byte[] EVs { get; set; }
        public byte HpEVs
        {
            get => EVs[PokemonBaseStats.hpStatIndex];
            set => EVs[PokemonBaseStats.hpStatIndex] = value;
        }
        public byte AttackEVs
        {
            get => EVs[PokemonBaseStats.atkStatIndex];
            set => EVs[PokemonBaseStats.atkStatIndex] = value;
        }
        public byte DefenseEVs
        {
            get => EVs[PokemonBaseStats.defStatIndex];
            set => EVs[PokemonBaseStats.defStatIndex] = value;
        }
        public byte SpeedEVs
        {
            get => EVs[PokemonBaseStats.spdStatIndex];
            set => EVs[PokemonBaseStats.spdStatIndex] = value;
        }
        public byte SpAttackEVs
        {
            get => EVs[PokemonBaseStats.spAtkStatIndex];
            set => EVs[PokemonBaseStats.spAtkStatIndex] = value;
        }
        public byte SpDefenseEVs
        {
            get => EVs[PokemonBaseStats.spDefStatIndex];
            set => EVs[PokemonBaseStats.spDefStatIndex] = value;
        }

        public void ClearEvs()
        {
            Array.Clear(EVs, 0, EVs.Length);
        }
    }
}
