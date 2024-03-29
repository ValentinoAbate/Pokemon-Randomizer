﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Randomization;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Metadata
{
    public class TrainerThemeData
    {
        public enum TrainerTheme 
        {
            Typed,
            Untyped,
            SpecificPokemon,
            Legendaries,
            // Celebrity Trainer (Someone who wouldn't normally be in the leader position)
            // Weather
        }

        public TrainerTheme Theme { get; set; }

        public PokemonType[] Types { get; set; } = Array.Empty<PokemonType>();

        public PokemonType[] SecondaryTypes { get; set; } = Array.Empty<PokemonType>();

        public double PrimaryTypeChance { get; set; } = 1;

        public Pokemon[] SpecificPokemon { get; set; } = Array.Empty<Pokemon>();

        public void SetTypes(PokemonType[] primaryTypes, PokemonType[] secondaryTypes, double primaryChance)
        {
            Types = primaryTypes;
            SecondaryTypes = secondaryTypes;
            PrimaryTypeChance = primaryChance;
            // Set theme based on given types
            if(Types.Length <= 0)
            {
                Theme = primaryChance >= 1 || SecondaryTypes.Length <= 0 ? TrainerTheme.Untyped : TrainerTheme.Typed;
            }
            else
            {
                Theme = SecondaryTypes.Length <= 0 && primaryChance <= 0 ? TrainerTheme.Untyped : TrainerTheme.Typed;
            }
        }

        // Create the pokemon metrics for an individual pokemon on the trainer's team
        public List<Metric<Pokemon>> GetPokemonMetrics(Randomization.Random rand, PkmnRandomizer pkmnRand, IEnumerable<Pokemon> all, Pokemon pokemon, Trainer trainer, IDataTranslator dataT)
        {
            return new List<Metric<Pokemon>>() { new Metric<Pokemon>(Theme switch
            {
                TrainerTheme.Typed => TypedMetric(rand, pkmnRand, all, dataT),
                TrainerTheme.SpecificPokemon => new WeightedSet<Pokemon>(all.Where(SpecificPokemon.Contains)),
                TrainerTheme.Legendaries => new WeightedSet<Pokemon>(all.Where(PokemonUtils.IsLegendary)),
                _ => new WeightedSet<Pokemon>(all),
            }, 0, 1)};
        }

        private WeightedSet<Pokemon> TypedMetric(Randomization.Random rand, PkmnRandomizer pkmnRand, IEnumerable<Pokemon> all, IDataTranslator dataT)
        {
            var types = rand.RollSuccess(PrimaryTypeChance) ? Types : SecondaryTypes;
            return pkmnRand.TypeSimilarityGroup(all, new WeightedSet<PokemonType>(types));
        }

        public override string ToString()
        {
            return Theme switch
            {
                TrainerTheme.Typed => TypeString(),
                TrainerTheme.SpecificPokemon => $"Specific Pokemon: {string.Join(", ", SpecificPokemon)}",
                _ => Theme.ToString().ToUpper(),
            };
        }
        
        private string TypeString()
        {
            string ret = Types.Length > 0 ? string.Join('/', Types) : "Untyped";
            if (PrimaryTypeChance >= 1)
                return ret;
            int primaryTypePercent = (int)(PrimaryTypeChance * 100);
            ret += $" ({primaryTypePercent}%) / {(SecondaryTypes.Length > 0 ? string.Join('/', SecondaryTypes) : "Untyped")} ({100 - primaryTypePercent}%)";
            return ret;
        }
    }
}
