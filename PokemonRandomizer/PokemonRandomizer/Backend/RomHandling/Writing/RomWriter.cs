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

        // Write the input learnset starting at the given offset
        protected void WriteLearnSet(Rom rom, LearnSet moves, int offset)
        {
            rom.SaveAndSeekOffset(offset);
            foreach (var move in moves)
            {
                // Write the first byte of the move
                rom.WriteByte((byte)move.move);
                // if the move number is over 255, the last bit of the learn level byte is set to 1
                if ((int)move.move > 255)
                {
                    rom.WriteByte((byte)(move.learnLvl * 2 + 1));
                }
                else
                {
                    rom.WriteByte((byte)(move.learnLvl * 2));
                }
            }
            rom.WriteRepeating(0xff, 2); // Terminator (0xFFFF)
            rom.LoadOffset();
        }

        protected void WriteTmHmCompat(Rom rom, int offset, PokemonBaseStats pokemon, ref int[] compatBuffer)
        {
            if (compatBuffer.Length != pokemon.TMCompat.Count + pokemon.HMCompat.Count)
            {
                throw new System.ArgumentOutOfRangeException($"Unexpected TM/HM buffer length: {compatBuffer.Length}. Expected {pokemon.TMCompat.Count + pokemon.HMCompat.Count}");
            }
            for (int i = 0; i < pokemon.TMCompat.Count; i++)
            {
                compatBuffer[i] = pokemon.TMCompat[i] ? 1 : 0;
            }
            for (int i = 0; i < pokemon.HMCompat.Count; i++)
            {
                compatBuffer[i + pokemon.TMCompat.Count] = pokemon.HMCompat[i] ? 1 : 0;
            }
            rom.WriteBits(offset, 1, compatBuffer);
        }
    }
}
