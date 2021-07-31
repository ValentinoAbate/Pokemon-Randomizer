﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI
{
    using static Settings;
    using Backend.DataStructures;
    using PokemonRandomizer.Backend.Randomization;

    public class WildEncounterDataModel : DataModel
    {
        public List<string> ApplyEncounterBankMetricsTo { get; } = new List<string>
        {
            EncounterSet.Type.Surf.ToString(),
            EncounterSet.Type.Fish.ToString(),
            EncounterSet.Type.RockSmash.ToString(),
            EncounterSet.Type.Headbutt.ToString(),
        };
        public float EncounterBankTypeSharpness => 3;
        public float EncounterBankTypeFilter => 0.1f;
        public PokemonSettings PokemonSettings { get; } = new PokemonSettings()
        {
            BanLegendaries = true,
            Noise = 0.001f,
            Data = new List<MetricData>()
            {
                new MetricData(PokemonMetric.typeEncounterSet, 0),
                new MetricData(PokemonMetric.typeEncounterBankType, 0),
                new MetricData(PokemonMetric.typeIndividual, 1),
                new MetricData(PokemonMetric.powerIndividual, 1),
            }
        };
        public WildEncounterRandomizer.Strategy Strategy { get; set; } = WildEncounterRandomizer.Strategy.Unchanged;

        public WildEncounterDataModel()
        {
            foreach(var metricData in PokemonSettings.Data)
            {
                InitializeMetric(metricData);
            }
        }
        public void InitializeMetric(MetricData data)
        {
            if(data.DataSource == PokemonMetric.typeEncounterBankType)
            {
                data.Flags.AddRange(ApplyEncounterBankMetricsTo);
                data.Sharpness = EncounterBankTypeSharpness;
                data.Filter = EncounterBankTypeFilter;
            }
            else
            {
                data.Reset();
            }
        }
    }
}