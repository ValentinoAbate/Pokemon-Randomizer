//using PokemonRandomizer.Backend.EnumTypes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PokemonRandomizer.Backend.Metadata
//{
//    public class TrainerTypeCalculator
//    {
//        private readonly Dictionary<(string, string), PokemonType[]> nameClassOverrides = new();
//        private readonly Dictionary<string, PokemonType[]> namedOverrides = new();
//        private readonly Dictionary<string, PokemonType[]> classOverrides = new();
//        public PokemonType[] GetTypes(Trainer trainer, IDataTranslator dataT)
//        {
//            if(nameClassOverrides.ContainsKey((trainer.name, trainer.Class)))
//            {
//                return nameClassOverrides[(trainer.name, trainer.Class)];
//            }
//            if (namedOverrides.ContainsKey(trainer.name))
//            {
//                return namedOverrides[trainer.name];
//            }
//            if (classOverrides.ContainsKey(trainer.Class))
//            {
//                return classOverrides[trainer.Class];
//            }
            
//            // Calculate type based on type theming
//            var types = new Dictionary<PokemonType, int>();

//            // Get type occurence
//            foreach (var pokemon in trainer.pokemon)
//            {
//                var baseStats = dataT.GetBaseStats(pokemon.species);
//                if (types.ContainsKey(baseStats.OriginalPrimaryType))
//                {
//                    types[baseStats.OriginalPrimaryType]++;
//                }
//                else
//                {
//                    types.Add(baseStats.OriginalPrimaryType, 1);
//                }
//                // Originally single types, don't log primary type
//                if (baseStats.OriginallySingleTyped)
//                    continue;
//                if (types.ContainsKey(baseStats.OriginalSecondaryType))
//                {
//                    types[baseStats.OriginalSecondaryType]++;
//                }
//                else
//                {
//                    types.Add(baseStats.OriginalSecondaryType, 1);
//                }
//            }

//            // Find most occuring type
//            var max = new KeyValuePair<PokemonType, int>(PokemonType.Unknown, int.MinValue);
//            foreach (var type in types)
//            {
//                if (type.Value > max.Value)
//                {
//                    max = type;
//                }
//            }

//            if (max.Value == int.MinValue)
//            {
//                return Array.Empty<PokemonType>();
//            }
//            return new PokemonType[] { max.Key };
//        }
//    }
//}
