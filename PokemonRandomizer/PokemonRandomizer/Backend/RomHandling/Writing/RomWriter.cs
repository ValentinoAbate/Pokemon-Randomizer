using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    public abstract class RomWriter : RomHandler
    {
        protected readonly Dictionary<Item, Item> itemRemaps = new Dictionary<Item, Item>();
        public abstract Rom Write(RomData data, Rom originalRom, RomMetadata metadata, XmlManager info, Settings settings);

        protected void WriteBaseStatsSingle(PokemonBaseStats pokemon, int offset, Rom rom)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            // fill in stats (hp/at/df/sp/sa/sd)
            rom.WriteBlock(pokemon.stats);
            // convert types to bytes and write
            rom.WriteBlock(Array.ConvertAll(pokemon.types, (t) => (byte)t));
            rom.WriteByte(pokemon.catchRate);
            rom.WriteByte(pokemon.baseExpYield);
            // Next two bytes bits 0-11 are ev Yields, in chunks of 2
            rom.WriteBits(2, pokemon.evYields);
            WriteItem(rom, pokemon.heldItems[0]);
            WriteItem(rom, pokemon.heldItems[1]);
            rom.WriteByte(pokemon.genderRatio);
            rom.WriteByte(pokemon.eggCycles);
            rom.WriteByte(pokemon.baseFriendship);
            rom.WriteByte((byte)pokemon.growthType);
            rom.WriteByte((byte)pokemon.eggGroups[0]);
            rom.WriteByte((byte)pokemon.eggGroups[1]);
            rom.WriteByte((byte)AbilityToInternalIndex(pokemon.abilities[0]));
            rom.WriteByte((byte)AbilityToInternalIndex(pokemon.abilities[1]));
            rom.WriteByte(pokemon.safariZoneRunRate);
            rom.WriteByte((byte)(((byte)pokemon.searchColor << 1) + Convert.ToByte(pokemon.flip)));
            // Padding
            rom.SetBlock(2, 0x00);
            rom.LoadOffset();
        }

        protected void WriteItem(Rom rom, Item item)
        {
            rom.WriteUInt16(ItemToInternalIndex(RemapItem(item)));
        }
        protected Item RemapItem(Item item)
        {
            return itemRemaps.ContainsKey(item) ? itemRemaps[item] : item;
        }
    }
}
