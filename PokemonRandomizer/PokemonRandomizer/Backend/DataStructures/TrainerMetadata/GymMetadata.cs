using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.TrainerMetadata
{
    public class GymMetadata : TrainerOrganizationMetadata
    {
        public override PokemonType[] Types { get; protected set; } = Array.Empty<PokemonType>();
        public List<Trainer> Leaders { get; set; } = new List<Trainer>();
        public List<Trainer> GymTrainers { get; set; } = new List<Trainer>();

        public void CalculateType(IDataTranslator dataT)
        {
            if(Leaders.Count <= 0)
            { 
                Types = Array.Empty<PokemonType>();
                return;
            }

            var leader = Leaders[0];
            // If the trainer has override metadata, use that
            if(leader.TrainerTypeMetadata != null)
            {
                Types = new PokemonType[leader.TrainerTypeMetadata.Types.Length];
                Array.Copy(leader.TrainerTypeMetadata.Types, Types, Types.Length);
                return;
            }

            var types = new Dictionary<PokemonType, int>();

            // Get type occurence
            foreach (var pokemon in leader.pokemon)
            {
                var baseStats = dataT.GetBaseStats(pokemon.species);
                if (types.ContainsKey(baseStats.OriginalPrimaryType))
                {
                    types[baseStats.OriginalPrimaryType]++;
                }
                else
                {
                    types.Add(baseStats.OriginalPrimaryType, 1);
                }
                // Originally single types, don't log primary type
                if (baseStats.OriginallySingleTyped)
                    continue;
                if (types.ContainsKey(baseStats.OriginalSecondaryType))
                {
                    types[baseStats.OriginalSecondaryType]++;
                }
                else
                {
                    types.Add(baseStats.OriginalSecondaryType, 1);
                }
            }

            // Find most occuring type
            var max = new KeyValuePair<PokemonType, int>(PokemonType.Unknown, int.MinValue);
            foreach(var type in types)
            {
                if(type.Value > max.Value)
                {
                    max = type;
                }
            }

            if (max.Value == int.MinValue)
            {
                Types = Array.Empty<PokemonType>();
                return;
            }

            Types = new PokemonType[] { max.Key };
        }
    }
}
