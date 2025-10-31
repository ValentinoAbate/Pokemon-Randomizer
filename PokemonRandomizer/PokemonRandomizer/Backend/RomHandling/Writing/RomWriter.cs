using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public abstract class RomWriter : RomHandler
    {
        public abstract Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings);

        protected byte[] TypeEffectivenessChartToByteArray(TypeEffectivenessChart typeDefinitions)
        {
            var typeData = new List<byte>(typeDefinitions.Count * 3 + TypeEffectivenessChart.separatorSequence.Length + TypeEffectivenessChart.endSequence.Length);
            foreach (var (typePair, effectiveness) in typeDefinitions.TypeRelations)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)effectiveness);
            }
            typeData.AddRange(TypeEffectivenessChart.separatorSequence);
            foreach (var (typePair, effectiveness) in typeDefinitions.IgnoreAfterForesight)
            {
                typeData.Add((byte)typePair.attackingType);
                typeData.Add((byte)typePair.defendingType);
                typeData.Add((byte)effectiveness);
            }
            typeData.AddRange(TypeEffectivenessChart.endSequence);
            return typeData.ToArray();
        }
    }
}
