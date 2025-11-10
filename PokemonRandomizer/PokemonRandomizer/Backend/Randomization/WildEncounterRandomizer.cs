using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using static PokemonRandomizer.Backend.Randomization.MoveCompatibilityRandomizer;
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

        public void RandomizeEncounters(IEnumerable<Pokemon> pokemonSet, IEnumerable<MapEncounterData> encounterData, PokemonSettings settings, Strategy strategy)
        {
            if (strategy == Strategy.Unchanged)
                return;
            if (strategy == Strategy.Individual)
            {
                foreach (var data in encounterData)
                {
                    foreach(var set in data.EncounterSets)
                    {
                        // Encounter wide type occurence data
                        var typeOccurence = EncounterTypeOccurence(set);
                        foreach (var enc in set.RealEncounters)
                        {
                            // Create metrics
                            var metrics = CreateMetrics(pokemonSet, enc.Pokemon, set.type, typeOccurence, settings.Data);
                            // Choose pokemon
                            enc.Pokemon = pokeRand.RandomPokemon(pokemonSet, enc.Pokemon, metrics, settings, enc.Level);
                        }
                    }
                }
            }
            else if (strategy == Strategy.AreaOneToOne)
            {
                foreach (var data in encounterData)
                {
                    // Create the mapping
                    var mapping = new Dictionary<Pokemon, Pokemon>(data.RealEncounterCount);
                    MapEncounterSets(mapping, data, true, pokemonSet, settings);
                    // Remap
                    foreach (var set in data.EncounterSets)
                    {
                        RemapEncounter(set, mapping, settings);
                    }
                }
            }
            else if (strategy == Strategy.GlobalOneToOne)
            {
                // Set wide type occurence data
                var typeOccurence = PokemonMetrics.TypeOccurence(pokemonSet, dataT.GetBaseStats);
                // Create the mapping
                var mapping = new Dictionary<Pokemon, Pokemon>();
                foreach (var data in encounterData)
                {
                    MapEncounterSets(mapping, data, false, pokemonSet, settings, typeOccurence);
                }
                // Remap encounters
                foreach (var data in encounterData)
                {
                    // Remap the encounters
                    foreach (var set in data.EncounterSets)
                    {
                        RemapEncounter(set, mapping, settings);
                    }
                }
            }
        }

        private void MapEncounterSets(Dictionary<Pokemon, Pokemon> mapping, MapEncounterData data, bool useEncounterSetType, IEnumerable<Pokemon> pokemonSet, PokemonSettings settings, WeightedSet<PokemonType> typeOccurenceOverride = null)
        {
            // Remap the encounters
            foreach (var set in data.EncounterSets)
            {
                foreach (var enc in set.RealEncounters)
                {
                    if (mapping.ContainsKey(enc.Pokemon))
                        continue;
                    // Encounter wide type occurence data
                    var typeOccurence = typeOccurenceOverride ?? EncounterTypeOccurence(set);
                    // Create metrics
                    var metrics = CreateMetrics(pokemonSet, enc.Pokemon, useEncounterSetType ? set.type : EncounterSet.Type.None, typeOccurence, settings.Data);
                    // Choose pokemon
                    mapping.Add(enc.Pokemon, pokeRand.RandomPokemon(pokemonSet, enc.Pokemon, metrics, settings));
                }
            }
        }

        private void RemapEncounter(EncounterSet encounterSet, Dictionary<Pokemon, Pokemon> map, PokemonSettings settings)
        {
            foreach (var enc in encounterSet.RealEncounters)
            {
                enc.Pokemon = map[enc.Pokemon];
                if (settings.ForceHighestLegalEvolution)
                {
                    enc.Pokemon = evoUtils.MaxEvolution(enc.Pokemon, enc.Level, settings.RestrictIllegalEvolutions);
                }
                else if (settings.RestrictIllegalEvolutions)
                {
                    enc.Pokemon = evoUtils.CorrectImpossibleEvo(enc.Pokemon, enc.Level);
                }
            }
        }

        private WeightedSet<PokemonType> EncounterTypeOccurence(EncounterSet encounter)
        {
            return PokemonMetrics.TypeOccurence(encounter, e => dataT.GetBaseStats(e.Pokemon));
        }

        private IEnumerable<Metric<Pokemon>> CreateMetrics(IEnumerable<Pokemon> all, Pokemon pokemon, EncounterSet.Type slotType, WeightedSet<PokemonType> typeOccurence, IReadOnlyList<MetricData> data)
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

        private WeightedSet<Pokemon> GetEncounterBankType(IEnumerable<Pokemon> all, EncounterSet.Type slotType, MetricData data)
        {
            if (!data.Flags.Contains(slotType.ToString()) || !romMetrics.EncounterSlotTypeOccurence.ContainsKey(slotType))
                return null;
            return pokeRand.TypeSimilarityGroup(all, romMetrics.EncounterSlotTypeOccurence[slotType], data.Sharpness);
        }
    }
}
