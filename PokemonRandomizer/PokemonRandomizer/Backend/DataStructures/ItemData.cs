using PokemonRandomizer.Backend.EnumTypes;

namespace PokemonRandomizer.Backend.DataStructures
{
    using Utilities;
    public class ItemData
    {
        public const string unusedName = "????????";
        // Name length limit in RSE
        public const int nameLength = 14;
        public bool IsUnused => Name == unusedName;
        public string Name { get; set;  }
        public Item Item { get; set; }
        public int Price { get; set; }
        // Hold effect data
        public byte holdEffect;
        public byte param;
        public string Description { get; set; }
        public int descriptionOffset;
        public bool IsKeyItem => keyItemValue > 0;
        // In RSE:
        // 0: Not a key item
        // 1: Key item (visible)
        // 2: Key item (invisible) (ex: blue orb, oak's parcel)
        public byte keyItemValue;
        public bool RegisterableKeyItem { get; set; }
        // In RSE:
        // 1: General Items pocket
        // 3: TM Pocket
        // 5: Key Item Pocket
        public byte pocket;
        // Not sure what this means
        public byte type;
        public int fieldEffectOffset;
        // 32-bit Uint
        // In RSE:
        // 0: no battle usage
        // 1: use on pokemon (potion, antidote, blue flute, etc)
        // 2: just use (balls, X items, Poke Doll, etc)
        public int battleUsage;
        public int battleEffectOffset;
        // seems to indicate ball type, mail type, bike type (also is 1 on eon ticket for some reason)
        public int extraData;
        // Stored in a different location
        public int spriteOffset = Rom.nullPointer;
        public int paletteOffset = Rom.nullPointer;

        public string OriginalDescription { get; private set; }
        public bool ReformatDescription { get; set; } = false;

        public override string ToString()
        {
            return Name;
        }

        public void CopyTo(ItemData other)
        {
            other.Name = Name;
            other.Price = Price;
            other.holdEffect = holdEffect;
            other.param = param;
            other.Description = Description;
            other.descriptionOffset = descriptionOffset;
            other.keyItemValue = keyItemValue;
            other.RegisterableKeyItem = RegisterableKeyItem;
            other.pocket = pocket;
            other.type = type;
            other.fieldEffectOffset = fieldEffectOffset;
            other.battleUsage = battleUsage;
            other.battleEffectOffset = battleEffectOffset;
            other.extraData = extraData;
            other.spriteOffset = spriteOffset;
            other.paletteOffset = paletteOffset;
            other.OriginalDescription = OriginalDescription;
        }

        public void SetOriginalValues()
        {
            OriginalDescription = Description;
        }
    }
}
