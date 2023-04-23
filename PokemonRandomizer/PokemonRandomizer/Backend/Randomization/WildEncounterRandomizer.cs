using System.Collections.Generic;
using System.Linq;
using System;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using static Settings;
    public class WildEncounterRandomizer
    {
        public enum Strategy
        {
            Unchanged, // May want to replace with a different guard later
            Individual,
            AreaOneToOne,
            GlobalOneToOne,
        }

        private readonly PkmnRandomizer pokeRand;
        private readonly EvolutionUtils evoUtils;
        private readonly IDataTranslator dataT;
        private readonly RomMetrics romMetrics;
        public WildEncounterRandomizer(PkmnRandomizer pokeRand, EvolutionUtils evoUtils, RomMetrics romMetrics, IDataTranslator dataT)
        {
            this.pokeRand = pokeRand;
            this.evoUtils = evoUtils;
            this.dataT = dataT;
            this.romMetrics = romMetrics;
        }

        public void RandomizeEncounters(List<Pokemon> pokemonSet, IEnumerable<EncounterSet> encounters, PokemonSettings settings, Strategy strategy)
        {
            if (strategy == Strategy.Unchanged)
                return;
            if (strategy == Strategy.Individual)
            {
                foreach (var encounterSet in encounters)
                {
                    // Encounter wide type occurence data
                    var typeOccurence = EncounterTypeOccurence(encounterSet);
                    foreach (var enc in encounterSet)
                    {
                        // Create metrics
                        var metrics = CreateMetrics(pokemonSet, enc.pokemon, encounterSet.type, typeOccurence, settings.Data);
                        // Choose pokemon
                        enc.pokemon = pokeRand.RandomPokemon(pokemonSet, enc.pokemon, metrics, settings, enc.level);
                    }
                }
            }
            else if (strategy == Strategy.AreaOneToOne)
            {
                foreach (var encounterSet in encounters)
                {
                    // Encounter wide type occurence data
                    var typeOccurence = EncounterTypeOccurence(encounterSet);
                    // Get all unique species in the encounter set
                    var species = encounterSet.Select((e) => e.pokemon).Distinct();
                    // Create the mapping
                    var mapping = new Dictionary<Pokemon, Pokemon>(encounterSet.Count());
                    foreach(var pokemon in species)
                    {
                        // Create metrics
                        var metrics = CreateMetrics(pokemonSet, pokemon, encounterSet.type, typeOccurence, settings.Data);
                        // Choose pokemon
                        mapping.Add(pokemon, pokeRand.RandomPokemon(pokemonSet, pokemon, metrics, settings));
                    }
                    // Remap
                    RemapEncounter(encounterSet, mapping, settings);
                }
            }
            else if (strategy == Strategy.GlobalOneToOne)
            {
                // Set wide type occurence data
                var typeOccurence = PokemonMetrics.TypeOccurence(pokemonSet, dataT.GetBaseStats);
                // Create the mapping
                var mapping = new Dictionary<Pokemon, Pokemon>();
                foreach (var pokemon in pokemonSet)
                {
                    // Create metrics
                    var metrics = CreateMetrics(pokemonSet, pokemon, EncounterSet.Type.None, typeOccurence, settings.Data);
                    // Choose pokemon
                    mapping.Add(pokemon, pokeRand.RandomPokemon(pokemonSet, pokemon, metrics, settings));
                }
                // Remap the encounters
                foreach (var encounterSet in encounters)
                {
                    RemapEncounter(encounterSet, mapping, settings);
                }
            }
        }

        private void RemapEncounter(EncounterSet encounterSet, Dictionary<Pokemon, Pokemon> map, PokemonSettings settings)
        {
            foreach (var enc in encounterSet)
            {
                enc.pokemon = map[enc.pokemon];
                if (settings.ForceHighestLegalEvolution)
                {
                    enc.pokemon = evoUtils.MaxEvolution(enc.pokemon, enc.level, settings.RestrictIllegalEvolutions);
                }
                else if (settings.RestrictIllegalEvolutions)
                {
                    enc.pokemon = evoUtils.CorrectImpossibleEvo(enc.pokemon, enc.level);
                }
            }
        }

        private WeightedSet<PokemonType> EncounterTypeOccurence(EncounterSet encounter)
        {
            return PokemonMetrics.TypeOccurence(encounter, e => dataT.GetBaseStats(e.pokemon));
        }

        private List<Metric<Pokemon>> CreateMetrics(List<Pokemon> all, Pokemon pokemon, EncounterSet.Type slotType, WeightedSet<PokemonType> typeOccurence, IReadOnlyList<MetricData> data)
        {
            var metrics = pokeRand.CreateBasicMetrics(all, pokemon, data, out List<MetricData> specialData);
            foreach (var d in specialData)
            {
                WeightedSet<Pokemon> input = d.DataSource switch
                {
                    PokemonMetric.typeEncounterSet      => pokeRand.TypeSimilarityGroup(all, typeOccurence, d.Sharpness),
                    PokemonMetric.typeEncounterBankType => GetEncounterBankType(all, slotType, d),
                    _ => null,
                };
                if(input != null)
                {
                    metrics.Add(new Metric<Pokemon>(input, d.Filter, d.Priority));
                }
            }
            return metrics;
        }

        private WeightedSet<Pokemon> GetEncounterBankType(List<Pokemon> all, EncounterSet.Type slotType, MetricData data)
        {
            if (!data.Flags.Contains(slotType.ToString()) || !romMetrics.EncounterSlotTypeOccurence.ContainsKey(slotType))
                return null;
            return pokeRand.TypeSimilarityGroup(all, romMetrics.EncounterSlotTypeOccurence[slotType], data.Sharpness);
        }
    }
}
