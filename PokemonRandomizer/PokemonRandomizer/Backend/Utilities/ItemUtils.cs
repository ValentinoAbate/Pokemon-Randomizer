using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Utilities
{
    using EnumTypes;
    using PokemonRandomizer.Backend.DataStructures;

    public static class ItemUtils
    {
        static ItemUtils()
        {
            itemToGenIVInternal = ReverseLookupUtils.BuildReverseLookup(genIVInternalToItem);
        }
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
        public static bool IsMinigameBerry(this Item item) => (item >= Item.Razz_Berry && item <= Item.Pinap_Berry) || (item >= Item.Cornn_Berry && item <= Item.Belue_Berry) || item == Item.Enigma_Berry;

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

        private static readonly Item[] genIVInternalToItem =
        {
            Item.None,
            Item.Master_Ball,
            Item.Ultra_Ball,
            Item.Great_Ball,
            Item.Poké_Ball,
            Item.Safari_Ball,
            Item.Net_Ball,
            Item.Dive_Ball,
            Item.Nest_Ball,
            Item.Repeat_Ball,
            Item.Timer_Ball,
            Item.Luxury_Ball,
            Item.Premier_Ball,
            Item.Dusk_Ball,
            Item.Heal_Ball,
            Item.Quick_Ball,
            Item.Cherish_Ball,
            Item.Potion,
            Item.Antidote,
            Item.Burn_Heal,
            Item.Ice_Heal,
            Item.Awakening,
            Item.Parlyz_Heal,
            Item.Full_Restore,
            Item.Max_Potion,
            Item.Hyper_Potion,
            Item.Super_Potion,
            Item.Full_Heal,
            Item.Revive,
            Item.Max_Revive,
            Item.Fresh_Water,
            Item.Soda_Pop,
            Item.Lemonade,
            Item.Moomoo_Milk,
            Item.Energypowder,
            Item.Energy_Root,
            Item.Heal_Powder,
            Item.Revival_Herb,
            Item.Ether,
            Item.Max_Ether,
            Item.Elixir,
            Item.Max_Elixir,
            Item.Lava_Cookie,
            Item.Berry_Juice,
            Item.Sacred_Ash,
            Item.HP_Up,
            Item.Protein,
            Item.Iron,
            Item.Carbos,
            Item.Calcium,
            Item.Rare_Candy,
            Item.PP_Up,
            Item.Zinc,
            Item.PP_Max,
            Item.Old_Gateau,
            Item.Guard_Spec,
            Item.Dire_Hit,
            Item.X_Attack,
            Item.X_Defend,
            Item.X_Speed,
            Item.X_Accuracy,
            Item.X_Special,
            Item.X_SpDef,
            Item.Poké_Doll,
            Item.Fluffy_Tail,
            Item.Blue_Flute,
            Item.Yellow_Flute,
            Item.Red_Flute,
            Item.Black_Flute,
            Item.White_Flute,
            Item.Shoal_Salt,
            Item.Shoal_Shell,
            Item.Red_Shard,
            Item.Blue_Shard,
            Item.Yellow_Shard,
            Item.Green_Shard,
            Item.Super_Repel,
            Item.Max_Repel,
            Item.Escape_Rope,
            Item.Repel,
            Item.Sun_Stone,
            Item.Moon_Stone,
            Item.Fire_Stone,
            Item.ThunderStone,
            Item.Water_Stone,
            Item.Leaf_Stone,
            Item.TinyMushroom,
            Item.Big_Mushroom,
            Item.Pearl,
            Item.Big_Pearl,
            Item.Stardust,
            Item.Star_Piece,
            Item.Nugget,
            Item.Heart_Scale,
            Item.Honey,
            Item.Growth_Mulch,
            Item.Damp_Mulch,
            Item.Stable_Mulch,
            Item.Gooey_Mulch,
            Item.Root_Fossil,
            Item.Claw_Fossil,
            Item.Helix_Fossil,
            Item.Dome_Fossil,
            Item.Old_Amber,
            Item.Armor_Fossil,
            Item.Skull_Fossil,
            Item.Rare_Bone,
            Item.Shiny_Stone,
            Item.Dusk_Stone,
            Item.Dawn_Stone,
            Item.Oval_Stone,
            Item.Odd_Keystone,
            Item.Griseous_Orb,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.None,
            Item.Adamant_Orb,
            Item.Lustrous_Orb,
            Item.Grass_Mail,
            Item.Flame_Mail,
            Item.Bubble_Mail,
            Item.Bloom_Mail,
            Item.Tunnel_Mail,
            Item.Steel_Mail,
            Item.Heart_Mail,
            Item.Snow_Mail,
            Item.Space_Mail,
            Item.Air_Mail,
            Item.Mosaic_Mail,
            Item.Brick_Mail,
            Item.Cheri_Berry,
            Item.Chesto_Berry,
            Item.Pecha_Berry,
            Item.Rawst_Berry,
            Item.Aspear_Berry,
            Item.Leppa_Berry,
            Item.Oran_Berry,
            Item.Persim_Berry,
            Item.Lum_Berry,
            Item.Sitrus_Berry,
            Item.Figy_Berry,
            Item.Wiki_Berry,
            Item.Mago_Berry,
            Item.Aguav_Berry,
            Item.Iapapa_Berry,
            Item.Razz_Berry,
            Item.Bluk_Berry,
            Item.Nanab_Berry,
            Item.Wepear_Berry,
            Item.Pinap_Berry,
            Item.Pomeg_Berry,
            Item.Kelpsy_Berry,
            Item.Qualot_Berry,
            Item.Hondew_Berry,
            Item.Grepa_Berry,
            Item.Tamato_Berry,
            Item.Cornn_Berry,
            Item.Magost_Berry,
            Item.Rabuta_Berry,
            Item.Nomel_Berry,
            Item.Spelon_Berry,
            Item.Pamtre_Berry,
            Item.Watmel_Berry,
            Item.Durin_Berry,
            Item.Belue_Berry,
            Item.Occa_Berry,
            Item.Passho_Berry,
            Item.Wacan_Berry,
            Item.Rindo_Berry,
            Item.Yache_Berry,
            Item.Chople_Berry,
            Item.Kebia_Berry,
            Item.Shuca_Berry,
            Item.Coba_Berry,
            Item.Payapa_Berry,
            Item.Tanga_Berry,
            Item.Charti_Berry,
            Item.Kasib_Berry,
            Item.Haban_Berry,
            Item.Colbur_Berry,
            Item.Babiri_Berry,
            Item.Chilan_Berry,
            Item.Liechi_Berry,
            Item.Ganlon_Berry,
            Item.Salac_Berry,
            Item.Petaya_Berry,
            Item.Apicot_Berry,
            Item.Lansat_Berry,
            Item.Starf_Berry,
            Item.Enigma_Berry,
            Item.Micle_Berry,
            Item.Custap_Berry,
            Item.Jaboca_Berry,
            Item.Rowap_Berry,
            Item.BrightPowder,
            Item.White_Herb,
            Item.Macho_Brace,
            Item.Exp_Share,
            Item.Quick_Claw,
            Item.Soothe_Bell,
            Item.Mental_Herb,
            Item.Choice_Band,
            Item.Kings_Rock,
            Item.Silverpowder,
            Item.Amulet_Coin,
            Item.Cleanse_Tag,
            Item.Soul_Dew,
            Item.DeepSeaTooth,
            Item.DeepSeaScale,
            Item.Smoke_Ball,
            Item.Everstone,
            Item.Focus_Band,
            Item.Lucky_Egg,
            Item.Scope_Lens,
            Item.Metal_Coat,
            Item.Leftovers,
            Item.Dragon_Scale,
            Item.Light_Ball,
            Item.Soft_Sand,
            Item.Hard_Stone,
            Item.Miracle_Seed,
            Item.BlackGlasses,
            Item.Black_Belt,
            Item.Magnet,
            Item.Mystic_Water,
            Item.Sharp_Beak,
            Item.Poison_Barb,
            Item.NeverMeltIce,
            Item.Spell_Tag,
            Item.TwistedSpoon,
            Item.Charcoal,
            Item.Dragon_Fang,
            Item.Silk_Scarf,
            Item.Upーgrade,
            Item.Shell_Bell,
            Item.Sea_Incense,
            Item.Lax_Incense,
            Item.Lucky_Punch,
            Item.Metal_Powder,
            Item.Thick_Club,
            Item.Stick,
            Item.Red_Scarf,
            Item.Blue_Scarf,
            Item.Pink_Scarf,
            Item.Green_Scarf,
            Item.Yellow_Scarf,
            Item.Wide_Lens,
            Item.Muscle_Band,
            Item.Wise_Glasses,
            Item.Expert_Belt,
            Item.Light_Clay,
            Item.Life_Orb,
            Item.Power_Herb,
            Item.Toxic_Orb,
            Item.Flame_Orb,
            Item.Quick_Powder,
            Item.Focus_Sash,
            Item.Zoom_Lens,
            Item.Metronome,
            Item.Iron_Ball,
            Item.Lagging_Tail,
            Item.Destiny_Knot,
            Item.Black_Sludge,
            Item.Icy_Rock,
            Item.Smooth_Rock,
            Item.Heat_Rock,
            Item.Damp_Rock,
            Item.Grip_Claw,
            Item.Choice_Scarf,
            Item.Sticky_Barb,
            Item.Power_Bracer,
            Item.Power_Belt,
            Item.Power_Lens,
            Item.Power_Band,
            Item.Power_Anklet,
            Item.Power_Weight,
            Item.Shed_Shell,
            Item.Big_Root,
            Item.Choice_Specs,
            Item.Flame_Plate,
            Item.Splash_Plate,
            Item.Zap_Plate,
            Item.Meadow_Plate,
            Item.Icicle_Plate,
            Item.Fist_Plate,
            Item.Toxic_Plate,
            Item.Earth_Plate,
            Item.Sky_Plate,
            Item.Mind_Plate,
            Item.Insect_Plate,
            Item.Stone_Plate,
            Item.Spooky_Plate,
            Item.Draco_Plate,
            Item.Dread_Plate,
            Item.Iron_Plate,
            Item.Odd_Incense,
            Item.Rock_Incense,
            Item.Full_Incense,
            Item.Wave_Incense,
            Item.Rose_Incense,
            Item.Luck_Incense,
            Item.Pure_Incense,
            Item.Protector,
            Item.Electirizer,
            Item.Magmarizer,
            Item.Dubious_Disc,
            Item.Reaper_Cloth,
            Item.Razor_Claw,
            Item.Razor_Fang,
            Item.TM01,
            Item.TM02,
            Item.TM03,
            Item.TM04,
            Item.TM05,
            Item.TM06,
            Item.TM07,
            Item.TM08,
            Item.TM09,
            Item.TM10,
            Item.TM11,
            Item.TM12,
            Item.TM13,
            Item.TM14,
            Item.TM15,
            Item.TM16,
            Item.TM17,
            Item.TM18,
            Item.TM19,
            Item.TM20,
            Item.TM21,
            Item.TM22,
            Item.TM23,
            Item.TM24,
            Item.TM25,
            Item.TM26,
            Item.TM27,
            Item.TM28,
            Item.TM29,
            Item.TM30,
            Item.TM31,
            Item.TM32,
            Item.TM33,
            Item.TM34,
            Item.TM35,
            Item.TM36,
            Item.TM37,
            Item.TM38,
            Item.TM39,
            Item.TM40,
            Item.TM41,
            Item.TM42,
            Item.TM43,
            Item.TM44,
            Item.TM45,
            Item.TM46,
            Item.TM47,
            Item.TM48,
            Item.TM49,
            Item.TM50,
            Item.TM51,
            Item.TM52,
            Item.TM53,
            Item.TM54,
            Item.TM55,
            Item.TM56,
            Item.TM57,
            Item.TM58,
            Item.TM59,
            Item.TM60,
            Item.TM61,
            Item.TM62,
            Item.TM63,
            Item.TM64,
            Item.TM65,
            Item.TM66,
            Item.TM67,
            Item.TM68,
            Item.TM69,
            Item.TM70,
            Item.TM71,
            Item.TM72,
            Item.TM73,
            Item.TM74,
            Item.TM75,
            Item.TM76,
            Item.TM77,
            Item.TM78,
            Item.TM79,
            Item.TM80,
            Item.TM81,
            Item.TM82,
            Item.TM83,
            Item.TM84,
            Item.TM85,
            Item.TM86,
            Item.TM87,
            Item.TM88,
            Item.TM89,
            Item.TM90,
            Item.TM91,
            Item.TM92,
            Item.HM01,
            Item.HM02,
            Item.HM03,
            Item.HM04,
            Item.HM05,
            Item.HM06,
            Item.HM07,
            Item.HM08,
            Item.Explorer_Kit,
            Item.Loot_Sack,
            Item.Rule_Book,
            Item.Poké_Radar,
            Item.Point_Card,
            Item.Journal,
            Item.Seal_Case,
            Item.Fashion_Case,
            Item.Seal,
            Item.Pal_Pad,
            Item.Works_Key,
            Item.Old_Charm,
            Item.Galactic_Key,
            Item.Red_Chain,
            Item.Town_Map,
            Item.VS_Seeker,
            Item.Coin_Case,
            Item.Old_Rod,
            Item.Good_Rod,
            Item.Super_Rod,
            Item.Sprayduck,
            Item.Poffin_Case,
            Item.Bicycle,
            Item.Suite_Key,
            Item.Oaks_Letter,
            Item.Lunar_Wing,
            Item.Member_Card,
            Item.Azure_Flute,
            Item.SS_Ticket,
            Item.Contest_Pass,
            Item.Magma_Stone,
            Item.Parcel,
            Item.Coupon_1,
            Item.Coupon_2,
            Item.Coupon_3,
            Item.Storage_Key,
            Item.SecretPotion,
            Item.Vs_Recorder,
            Item.Gracidea,
            Item.Secret_Key,
            Item.Apricorn_Box,
            Item.Unown_Report,
            Item.Berry_Pots,
            Item.Dowsing_MCHN,
            Item.Blue_Card,
            Item.SlowpokeTail,
            Item.Clear_Bell,
            Item.Card_Key,
            Item.Basement_Key,
            Item.SquirtBottle,
            Item.Red_Scale,
            Item.Lost_Item,
            Item.Pass,
            Item.Machine_Part,
            Item.Silver_Wing,
            Item.Rainbow_Wing,
            Item.Mystery_Egg,
            Item.Red_Apricorn,
            Item.Yellow_Apricorn,
            Item.Blue_Apricorn,
            Item.Green_Apricorn,
            Item.Pink_Apricorn,
            Item.White_Apricorn,
            Item.Black_Apricorn,
            Item.Fast_Ball,
            Item.Level_Ball,
            Item.Lure_Ball,
            Item.Heavy_Ball,
            Item.Love_Ball,
            Item.Friend_Ball,
            Item.Moon_Ball,
            Item.Sport_Ball,
            Item.Park_Ball,
            Item.Photo_Album,
            Item.GB_Sounds,
            Item.Tidal_Bell,
            Item.RageCandyBar,
            Item.Data_Card_01,
            Item.Data_Card_02,
            Item.Data_Card_03,
            Item.Data_Card_04,
            Item.Data_Card_05,
            Item.Data_Card_06,
            Item.Data_Card_07,
            Item.Data_Card_08,
            Item.Data_Card_09,
            Item.Data_Card_10,
            Item.Data_Card_11,
            Item.Data_Card_12,
            Item.Data_Card_13,
            Item.Data_Card_14,
            Item.Data_Card_15,
            Item.Data_Card_16,
            Item.Data_Card_17,
            Item.Data_Card_18,
            Item.Data_Card_19,
            Item.Data_Card_20,
            Item.Data_Card_21,
            Item.Data_Card_22,
            Item.Data_Card_23,
            Item.Data_Card_24,
            Item.Data_Card_25,
            Item.Data_Card_26,
            Item.Data_Card_27,
            Item.Jade_Orb,
            Item.Lock_Capsule,
            Item.Red_Orb,
            Item.Blue_Orb,
            Item.Enigma_Stone,
    };
        private static readonly Dictionary<Item, int> itemToGenIVInternal;

        public static Item Gen4InternalToItem(int gen4InternalIndex)
        {
            return genIVInternalToItem[gen4InternalIndex];
        }

        public static int ItemToGen4Internal(Item item)
        {
            return itemToGenIVInternal[item];
        }

        public static bool IsQuantity(Item item)
        {
            return item is Item.Sacred_Ash or Item.Stardust or Item.Berry_Juice or Item.BrightPowder 
                        or Item.Silverpowder or Item.Gold_Teeth or Item.Tea or Item.Honey or Item.Growth_Mulch
                        or Item.Damp_Mulch or Item.Stable_Mulch or Item.Gooey_Mulch;
        }
    }
}
