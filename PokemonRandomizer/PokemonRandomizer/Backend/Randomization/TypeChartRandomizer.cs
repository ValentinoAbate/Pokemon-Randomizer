using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    public class TypeChartRandomizer
    {
        public enum Option
        {
            None,
            Invert,
            Swap
        }
        public void RandomizeTypeChart(TypeEffectivenessChart chart, Option option, Dictionary<string, List<string>> randomizationResults)
        {
            if(option == Option.Invert)
            {
                InvertTypeChart(chart.TypeRelations);
                InvertTypeChart(chart.IgnoreAfterForesight);
                LogResults(chart, randomizationResults);
            }
            else if (option == Option.Swap)
            {
                SwapTypeChart(chart.TypeRelations);
                SwapTypeChart(chart.IgnoreAfterForesight);
                LogResults(chart, randomizationResults);
            }
        }

        private void SwapTypeChart(Dictionary<TypeEffectivenessChart.TypePair, TypeEffectiveness> chart)
        {
            var swapped = new Dictionary<TypeEffectivenessChart.TypePair, TypeEffectiveness>(chart.Count);
            foreach (var (typePair, effectiveness) in chart)
            {
                swapped.Add(new TypeEffectivenessChart.TypePair(typePair.defendingType, typePair.attackingType), effectiveness);
            }
            chart.Clear();
            foreach(var (typePair, effectiveness) in swapped)
            {
                chart.Add(typePair, effectiveness);
            }
        }

        // Invert type effectivenes (like in inverse battles)
        private void InvertTypeChart(Dictionary<TypeEffectivenessChart.TypePair,TypeEffectiveness> chart)
        {
            foreach(var (typePair, effectiveness) in chart)
            {
                chart[typePair] = InvertEffectiveness(effectiveness);
            }
        }

        private TypeEffectiveness InvertEffectiveness(TypeEffectiveness t)
        {
            return t switch
            {
                TypeEffectiveness.NoEffect or TypeEffectiveness.NotVeryEffective => TypeEffectiveness.SuperEffective,
                TypeEffectiveness.SuperEffective => TypeEffectiveness.NotVeryEffective,
                _ => TypeEffectiveness.Normal
            };
        }

        private void LogResults(TypeEffectivenessChart chart, Dictionary<string, List<string>> randomizationResults)
        {
            var normalResults = new List<string>(chart.TypeRelations.Count);
            foreach(var (typePair, effectiveness) in chart.TypeRelations)
            {
                normalResults.Add($"{typePair.attackingType} -> {typePair.defendingType}: {effectiveness}");
            }
            randomizationResults.Add("Type Effectiveness", normalResults);

            var foresightResults = new List<string>(chart.IgnoreAfterForesight.Count);
            foreach (var (typePair, effectiveness) in chart.IgnoreAfterForesight)
            {
                foresightResults.Add($"{typePair.attackingType} -> {typePair.defendingType}: {effectiveness}");
            }
            randomizationResults.Add("Type Effectiveness (ignore after foresight)", foresightResults);
        }
    }
}
