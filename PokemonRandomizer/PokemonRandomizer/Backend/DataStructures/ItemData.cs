using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class ItemData
    {
        public const string unusedName = "????????";
        // Name length limit in RSE
        public const int nameLength = 14;

        [System.Flags]
        public enum Categories
        {
            None          = 0,
            KeyItem       = 1 << 0,
            Ball          = 1 << 1,
            Medicine      = 1 << 2,
            TM            = 1 << 3,
            HM            = 1 << 4,
            Mail          = 1 << 5,
            Berry         = 1 << 6,
            HeldItem      = 1 << 7,
            BattleItem    = 1 << 8,
            BattleBerry   = 1 << 9,
            EVBerry       = 1 << 10,
            MinigameBerry = 1 << 11, // Berry that are only used for pokeblocks, powder, poffins, etc.
            SellItem      = 1 << 12,
            StatIncrease  = 1 << 13, // (Protein, PP up, Rare Candy, etc)
            ExchangeItem  = 1 << 14, // (Shards (RSE), Heart Scale (RSE), TinyMushroom (FRLG), Shoal Items (RSE))
            ContestScarf  = 1 << 15,
            Utility       = 1 << 16, // (Escape Rope, Repels, Poke Doll, Etc)
            Breeding      = 1 << 17, // (Inscence, Light Ball, etc)
            LuckyEgg      = 1 << 18,
            EvolutionItem = 1 << 19,
            Special       = 1 << 20,
            Flute         = 1 << 21,
        }

        public bool IsUnused => Name == unusedName;
        public string Name { get; set;  }
        public Item Item { get; set; }
        public int Price { get; set; }
        public int OriginalPrice { get; set; }
        // Hold effect data
        public HeldItemEffect HoldEffect { get; set; }
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
        public bool IsMysteryGiftEventItem { get; set; }
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
        public Categories ItemCategories { get; set; } = Categories.None;

        public override string ToString()
        {
            return Name;
        }

        public void CopyTo(ItemData other)
        {
            other.Name = Name;
            other.Price = Price;
            other.HoldEffect = HoldEffect;
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
            other.OriginalPrice = OriginalPrice;
            other.ItemCategories = ItemCategories;
        }

        public void SetOriginalValues()
        {
            OriginalDescription = Description;
            OriginalPrice = Price;
        }

        // Later can add specific items that count as certain things for this ROM, etc
        public void SetCategoryFlags()
        {
            if (IsUnused)
            {
                return;
            }
            if (IsKeyItem)
            {
                ItemCategories = Categories.KeyItem;
                if (Item.IsHM())
                {
                    ItemCategories |= Categories.HM;
                }
            }
            else if (Item.IsMedicine())
            {
                ItemCategories = Categories.Medicine;
            }
            else if (Item.IsPokeBall())
            {
                ItemCategories = Categories.Ball;
            }
            else if (Item.IsTM())
            {
                ItemCategories = Categories.TM;
            }
            else if (Item.IsMail())
            {
                ItemCategories = Categories.Mail;
            }
            else if(Item.IsExchangeItem())
            {
                ItemCategories = Categories.ExchangeItem;
            }
            else if (Item.IsContestScarf())
            {
                ItemCategories = Categories.ContestScarf;
            }
            else if (Item.IsSellItem())
            {
                ItemCategories = Categories.SellItem;
            }
            else if (Item.IsStatBoostItem())
            {
                ItemCategories = Categories.StatIncrease;
            }
            else if (Item.IsBattleItem())
            {
                ItemCategories = Categories.BattleItem;
            }
            else if (Item.IsUtilityItem())
            {
                ItemCategories = Categories.Utility;
            }
            else if (Item.IsBerry())
            {
                ItemCategories = Categories.Berry;
                if (HoldEffect != HeldItemEffect.None)
                {
                    ItemCategories |= Categories.BattleBerry;
                }
                if (Item.IsEvBerry())
                {
                    ItemCategories |= Categories.EVBerry;
                }
                else if (Item.IsMinigameBerry())
                {
                    ItemCategories |= Categories.MinigameBerry;
                }
            }

            if (Item.IsSpecialItem())
            {
                ItemCategories |= Categories.Special;
            }
            if (Item.IsHeldItem())
            {
                ItemCategories |= Categories.HeldItem;
                if (Item.IsBreedingItem())
                {
                    ItemCategories |= Categories.Breeding;
                }
                if (Item == Item.Lucky_Egg)
                {
                    ItemCategories |= Categories.LuckyEgg;
                }
            }
            if (Item.IsFlute())
            {
                ItemCategories |= Categories.Flute;
            }
            if (Item.IsEvolutionItem())
            {
                ItemCategories |= Categories.EvolutionItem;
            }

        }
    }
}
