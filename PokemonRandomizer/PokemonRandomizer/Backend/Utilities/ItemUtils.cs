using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    using EnumTypes;
    using PokemonRandomizer.Backend.DataStructures;

    public static class ItemUtils
    {
        private static readonly HashSet<Item> mail = new HashSet<Item>
        {
            Item.Bead_Mail,
            Item.Dream_Mail,
            Item.Fab_Mail,
            Item.Glitter_Mail,
            Item.Harbor_Mail,
            Item.Mech_Mail,
            Item.Orange_Mail,
            Item.Retro_Mail,
            Item.Shadow_Mail,
            Item.Tropic_Mail,
            Item.Wave_Mail,
            Item.Wood_Mail,
        };
        public static bool IsMail(this ItemData item)
        {
            return IsMail(item.Item);
        }
        public static bool IsMail(this Item item)
        {
            return mail.Contains(item);
        }
        private static readonly HashSet<Item> tms = new HashSet<Item>
        {
            Item.TM01, Item.TM02, Item.TM03, Item.TM04, Item.TM05,
            Item.TM06, Item.TM07, Item.TM08, Item.TM09, Item.TM10,
            Item.TM11, Item.TM12, Item.TM13, Item.TM14, Item.TM15,
            Item.TM16, Item.TM17, Item.TM18, Item.TM19, Item.TM20,
            Item.TM21, Item.TM22, Item.TM23, Item.TM24, Item.TM25,
            Item.TM26, Item.TM27, Item.TM28, Item.TM29, Item.TM30,
            Item.TM31, Item.TM32, Item.TM33, Item.TM34, Item.TM35,
            Item.TM36, Item.TM37, Item.TM38, Item.TM39, Item.TM40,
            Item.TM41, Item.TM42, Item.TM43, Item.TM44, Item.TM45,
            Item.TM46, Item.TM47, Item.TM48, Item.TM49, Item.TM50,
        };
        public static bool IsTM(this ItemData item)
        {
            return IsTM(item.Item);
        }
        public static bool IsTM(this Item item)
        {
            return tms.Contains(item);
        }

        private static readonly HashSet<Item> hms = new HashSet<Item>
        {
            Item.HM01, Item.HM02, Item.HM03, Item.HM04, Item.HM05,
            Item.HM06, Item.HM07, Item.HM08, 
        };
        public static bool IsHM(this Item item) => hms.Contains(item);

        private static readonly HashSet<Item> pokeBalls = new HashSet<Item>
        {
            Item.Poké_Ball, Item.Great_Ball, Item.Dive_Ball, Item.Luxury_Ball, Item.Master_Ball, Item.Nest_Ball,
            Item.Net_Ball, Item.Premier_Ball, Item.Repeat_Ball, Item.Safari_Ball, Item.Timer_Ball, Item.Ultra_Ball
        };
        public static bool IsPokeBall(this Item item) => pokeBalls.Contains(item);

        private static readonly HashSet<Item> contestScarves = new HashSet<Item>
        {
            Item.Blue_Scarf, Item.Green_Scarf, Item.Pink_Scarf, Item.Red_Scarf, Item.Yellow_Scarf
        };
        public static bool IsContestScarf(this Item item) => contestScarves.Contains(item);

        public static bool IsShoalMaterial(this Item item) => item == Item.Shoal_Salt || item == Item.Shoal_Shell;

        private static readonly HashSet<Item> shards = new HashSet<Item>
        {
            Item.Blue_Shard, Item.Green_Shard, Item.Red_Shard, Item.Yellow_Shard
        };
        public static bool IsShard(this Item item) => shards.Contains(item);

        public static bool IsExchangeItem(this Item item) => item.IsShard() || item.IsShoalMaterial() || item == Item.Heart_Scale || item == Item.TinyMushroom;

        private static readonly HashSet<Item> sellItems = new HashSet<Item>
        {
            Item.Nugget, Item.Pearl, Item.Big_Pearl, Item.Stardust, Item.Star_Piece, Item.Big_Mushroom
        };
        public static bool IsSellItem(this Item item) => sellItems.Contains(item);

        private static readonly HashSet<Item> statBoostItems = new HashSet<Item>
        {
            Item.PP_Up, Item.PP_Max, Item.Rare_Candy, Item.Protein, Item.Zinc, Item.Calcium, Item.Carbos, Item.Iron, Item.HP_Up
        };
        public static bool IsStatBoostItem(this Item item) => statBoostItems.Contains(item);

        private static readonly HashSet<Item> specialItems = new HashSet<Item>
        {
            Item.PP_Max, Item.Master_Ball, Item.Liechi_Berry
        };
        public static bool IsSpecialItem(this Item item) => specialItems.Contains(item);

        private static readonly HashSet<Item> battleItems = new HashSet<Item>
        {
            Item.X_Attack, Item.X_Defend, Item.X_Special, Item.X_Speed, Item.X_Accuracy, Item.Guard_Spec, Item.Dire_Hit
        };
        public static bool IsBattleItem(this Item item) => battleItems.Contains(item);

        public static bool IsBerry(this Item item) => item >= Item.Cheri_Berry && item <= Item.Enigma_Berry;
        public static bool IsEvBerry(this Item item) => item >= Item.Pomeg_Berry && item <= Item.Tamato_Berry;
        public static bool IsMinigameBerry(this Item item) => (item >= Item.Razz_Berry && item <= Item.Pinap_Berry) || (item >= Item.Cornn_Berry && item <= Item.Durin_Berry) || item == Item.Enigma_Berry;

        public static bool IsHeldItem(this Item item) => (item >= Item.BrightPowder && item <= Item.Stick) && item != Item.Upーgrade && item != Item.Dragon_Scale;

        private static readonly HashSet<Item> flutes = new HashSet<Item>
        {
            Item.Black_Flute, Item.White_Flute, Item.Blue_Flute, Item.Red_Flute, Item.Yellow_Flute, Item.Poke_Flute
        };
        public static bool IsFlute(this Item item) => flutes.Contains(item);

        private static readonly HashSet<Item> utilityItems = new HashSet<Item>
        {
            Item.Repel, Item.Super_Repel, Item.Max_Repel, Item.Escape_Rope, Item.Fluffy_Tail, Item.Poké_Doll
        };
        public static bool IsUtilityItem(this Item item) => utilityItems.Contains(item);

        public static bool IsMedicine(this Item item) => (item >= Item.Potion && item <= Item.Lava_Cookie) || item == Item.Berry_Juice || item == Item.Sacred_Ash;

        private static readonly HashSet<Item> evolutionItems = new HashSet<Item>
        {
            Item.Fire_Stone, Item.Leaf_Stone, Item.Water_Stone, Item.ThunderStone, Item.Moon_Stone, Item.Sun_Stone,
            Item.Metal_Coat, Item.Dragon_Scale, Item.Kings_Rock, Item.Upーgrade, Item.DeepSeaScale, Item.DeepSeaTooth
        };
        public static bool IsEvolutionItem(this Item item) => evolutionItems.Contains(item);

        private static readonly HashSet<Item> breedingItems = new HashSet<Item>
        { 
            Item.Lax_Incense, Item.Sea_Incense, Item.Light_Ball, Item.Everstone
        };
        public static bool IsBreedingItem(this Item item) => breedingItems.Contains(item);
    }
}
