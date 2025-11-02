using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class MoveData
    {
        public enum Type
        {
            Physical,
            Special,
            Status,
            Unknown,
            TypeDependent,
        }
        public enum MoveEffect
        {
            Damage,
            StatusSleep,
            DamagePoisonChance,
            DamageStealHp,
            DamageBurnChance,
            DamageFreezeChance,
            DamageParalyzeChance,
            Selfdestruct,
            DreamEater,
            MirrorMove,
            AtkPlus1,
            DefPlus1,
            SpdPlus1,
            SpAtkPlus1,
            SpDefPlus1,
            AccPlus1,
            EvadePlus1,
            DamageAlwaysHit,
            AtkMinus1,
            DefMinus1,
            SpdMinus1,
            SpAtkMinus1,
            SpDefMinus1,
            AccMinus1,
            EvadeMinus1,
            Haze,
            Bide,
            AttackUntilConfused,
            ForceSwitch,
            Multihit,
            Conversion,
            DamageFlinchChance,
            RecoverHp,
            StatusBadlyPoison,
            PayDay,
            LightScreen,
            TriAttack,
            Rest,
            OneHitKill,
            ChargeTurn,
            HalfHp,
            FlatDamage40,
            DoTTrap,
            DamageHighCrit,
            DamageTwoHit,
            DamageMissHurtsSelf,
            Mist,
            StatusCritRateUp,
            DamageRecoilOneFourth,
            StatusConfuse,
            AtkPlus2,
            DefPlus2,
            SpdPlus2,
            SpAtkPlus2,
            SpDefPlus2,
            AccPlus2,
            EvadePlus2,
            Transform,
            AtkMinus2,
            DefMinus2,
            SpdMinus2,
            SpAtkMinus2,
            SpDefMinus2,
            AccMinus2,
            EvadeMinus2,
            Reflect,
            StatusPoison,
            StatusParalyze,
            DamageAtkMinus1Chance,
            DamageDefMinus1Chance,
            DamageSpdMinus1Chance,
            DamageSpAtkMinus1Chance,
            DamageSpDefMinus1Chance,
            DamageAccMinus1Chance,
            DamageEvadeMinus1Chance,
            SkyAttack,
            DamageConfuseChance,
            DamageTwoHitPoisonChance,
            DamagePriorityNeg1AlwaysHit,
            Substitute,
            DamageTiredAfterUse,
            Rage,
            Mimic,
            Metronome,
            StatusLeechSeed,
            DoNothing,
            Disable,
            FlatDamageLevel,
            VaryingDamageLevel,
            Counter,
            Encore,
            PainSplit,
            DamageFailUnlessAsleepFlinchChance,
            Conversion2,
            NextMoveAlwaysHits,
            Sketch,
            SleepTalk = 97,
            DestinyBond,
            DamageMoreAtLowHP,
            Spite,
            DamageCannotKill,
            CurePartyStatus,
            DamageHighPriority,
            DamageThreeConsecutiveHits,
            DamageStealHeldItem,
            Trap,
            StatusNightmare,
            EvadePlus1AndMinimize,
            Curse,
            Protect = 111,
            Spikes,
            Foresight,
            PerishSong,
            WeatherSandstorm,
            Endure,
            MultiTurnBuildup, // Rollout, Ice Ball
            StatusConfuseAtkPlus2,
            FuryCutter,
            Attract,
            DamagePowerUpIfFriendly,
            Present,
            DamagePowerDownIfFriendly,
            Safeguard,
            DamageBurnChanceThaw,
            Magnitude,
            BatonPass,
            Pursuit,
            DamageRemoveHazardsAndBinding, // Rapid Spin
            FlatDamage20,
            RecoverHpWeather1 = 132,
            RecoverHpWeather2,
            RecoverHpWeather3,
            HiddenPower,
            WeatherRain,
            WeatherSun,
            DamageDefPlus1Chance,
            DamageAtkPlus1Chance,
            DamageAllStatsPlus1Chance,
            HalfHpMaxAttack = 142,
            PsychUp,
            MirrorCoat,
            SkullBash,
            DamageHitSky2xFlinchChance,
            DamageHitUnderground2x,
            FutureAttack,
            DamageHitSky2x,
            DamageFlinchChanceHitMinimized2x,
            Solarbeam,
            Thunder,
            Teleport,
            MultiHitPartyMembers,
            DamageSpendTurnInSkyOrUnderground, // Gen 3: Fly, Dive, Bounce, and Dig
            DefPlus1AndPrepForRoll,
            Softboiled,
            DamagePriorityFlinchChance1stTurnOnly, // Fake out
            Uproar,
            Stockpile,
            SpitUp,
            Swallow,
            WeatherHail = 164,
            Torment,
            StatusConfuseSpAtkPlus2,
            StatusBurn,
            Memento,
            Facade,
            FocusPunch,
            SmellingSalt,
            FollowMe,
            NaturePower,
            Charge,
            Taunt,
            HelpingHand,
            SwapItems,
            RolePlay,
            Wish,
            Assist,
            Ingrain,
            Superpower,
            MagicCoat,
            Recycle,
            Revenge,
            DamageBreakBarriers,
            Yawn,
            KnockOff,
            Endeavor,
            DamageLessAtLowHp,
            SkillSwap,
            Imprison,
            CureStatus,
            Grudge,
            Snatch,
            DamageWeightBased,
            SecretPower,
            RecoilOneThird,
            StatusConfuseAll,
            DamageHighCritBurnChance,
            MudSport,
            DamageBadlyPoisonChance,
            WeatherBall,
            DamageSpAtkMinus2,
            AtkDefMinus1,
            DefSpDefPlus1,
            DamageHitSky,
            AtkDefPlus1,
            DamageHighCritPoisonChance,
            WaterSport,
            SpAtkSpDefPlus1,
            DragonDance,
            Camouflage,
            Roost,
            Gravity,
            MiracleEye, // Remove dark immunity and ignore ACC
            WakeupSlap,
            DamageSpdMinus1, // Hammer arm
            DamageMoreLowerSpeed,
            HealingWish,
            DamageDoubleIfTargetBelowHalfHp,
            NaturalGift,
            Feint,
            DamageEatBerry,
            Tailwind,
            RandomStatPlus2,
            MetalBurst,
            DamageThenSwitchOut,
            DamageDefSpDefMinus1, // Close Combat, etc.
            DamageDoubleIfAfterTarget, // Payback
            DamageDoubleIfTargetAlreadyHit, // Assurance
            Embargo,
            Fling,
            TransferStatus,
            DamageHigherWhenLowPP,
            HealBlock,
            DamageMoreIfTargetHighHp,
            SwapAtkDef,
            SupressAbility,
            LuckyChant,
            MeFirst,
            CopyCat,
            SwapAtkSpAtkChanges,
            SwapDefSpDefChanges,
            DamageTargetStatUpDependent,
            DamageFailIfNotLastMove,
            SetAbilityToInsomnia,
            DamageHitFirstIfTargetAttacking, // Sucker punch
            ToxicSpikes,
            SwapStatChanges,
            RestoreHpEveryTurn,
            MagnetRise,
            DamageRecoilOneThirdBurnChanceThaw,
            Struggle,
            DamageSpendTurnUnderwater,
            DamageSpendTurnUnderground,
            DamageHitUnderwater2x,
            ClearFieldEvadeMinus1,
            TrickRoom,
            Blizzard,
            DoTTrapHitUnderwater2x,
            DamageRecoilOneThirdParalyzeChance,
            DamageSpendTurnInSkyParalyzeChance,
            SpAtkMinus2OppositeGender = 265,
            StealthRock,
            Chatter,
            Judgement,
            DamageRecoilOneHalf,
            LunarDance,
            DamageSpAtkMinus2Chance,
            DamageSpendTurnInShadowRealm,
            DamageFlinchBurnChance,
            DamageFlinchFreezeChance,
            DamageFlinchParalyzeChance,
            DamageSpAtkPlus1Chance,

            ChargeTurnHighCrit, // Razor wind (Gen IV+)
            EvadePlus2AndMinimize, // Minimize (Gen IV+)
            DamageSpendTurnInSky, // Fly (Gen IV+)
        }
        public enum Targets
        {
            SelectedTarget = 0,
            Special = 1, // Specific to certain attacks (counter is enemy, metronone could hit anything)
            Unused = 2,
            Random = 4,
            BothEnemies = 8,
            Self = 16,
            EnemiesAndPartner = 32,
            OpponentField = 64, // For moves like spikes
        }

        private static readonly Dictionary<Move, PokemonType[]> moveTypeOverrides = new Dictionary<Move, PokemonType[]>()
        {
            { Move.SMOKESCREEN, new PokemonType[] { PokemonType.FIR } },
            { Move.WHIRLWIND, new PokemonType[] { PokemonType.FLY } },
            { Move.HARDEN, new PokemonType[] { PokemonType.RCK, PokemonType.BUG } },
            { Move.GROWTH, new PokemonType[] { PokemonType.GRS } },
            { Move.SWEET_SCENT, new PokemonType[] { PokemonType.GRS } },
            { Move.MEAN_LOOK, new PokemonType[] { PokemonType.GHO } },
            { Move.CURSE, new PokemonType[] { PokemonType.GHO } },
            { Move.POISONPOWDER, new PokemonType[] { PokemonType.GRS, PokemonType.PSN } },
            { Move.FLASH, new PokemonType[] { PokemonType.ELE, PokemonType.PSY } },
            { Move.WILLーOーWISP, new PokemonType[]{ PokemonType.FIR, PokemonType.GHO } },
            { Move.SANDSTORM, new PokemonType[]{ PokemonType.RCK, PokemonType.GRD } },
        };

        public bool IsType(PokemonType t)
        {
            // Check for a type override
            if (moveTypeOverrides.ContainsKey(move))
            {
                foreach(var t2 in moveTypeOverrides[move])
                {
                    if (t == t2)
                    {
                        return true;
                    }
                }
                return false;
            }
            return t == type;
        }
        public bool IsType(PokemonBaseStats pokemon)
        {
            return IsType(pokemon.PrimaryType) || (pokemon.IsDualTyped && IsType(pokemon.SecondaryType));
        }
        public bool IsOriginalType(PokemonBaseStats pokemon)
        {
            return IsType(pokemon.OriginalPrimaryType) || (pokemon.OriginallyDualTyped && IsType(pokemon.OriginalSecondaryType));
        }

        // Data structure documentation: https://bulbapedia.bulbagarden.net/wiki/Move_data_structure_in_Generation_III

        public Move move;
        public MoveEffect effect;
        public byte power;
        public PokemonType type;
        public byte accuracy;
        public byte pp;
        public byte effectChance;
        public Targets targets;
        // This byte is signed, If it is strictly less than 0x80 (128) then it is positive. 
        // If not, the actual value equals: -1 * (256 - Current Value). Example: value 0xFE (254) must be treated as -2 instead.
        public byte priority;
        // Flags documentation
        public bool Contact { get => flags[0]; }               // 0 - This moves makes contact with the target.
        public bool AffectedByProtect { get => flags[1]; }     // 1 - This move is affected by Protect.
        public bool AffectedByMagicCoat { get => flags[2]; }   // 2 - This move is affected by Magic Coat.
        public bool AffectedBySnatch { get => flags[3]; }      // 3 - This move is affected by Snatch.Note that this is mutually-exclusive with flag 2.
        public bool CanBeUsedByMirrorMove { get => flags[4]; } // 4 - This move may be used with Mirror Move
        public bool AffectedByKingRock { get => flags[5]; }    // 5 - This move is affected by the effects of King's Rock. The flinch effect is considered an additional effect for the purposes of Shield Dust, but not Serene Grace
        public BitArray flags;

        public bool AffectedByStab => !IsStatus && !IsOneHitKO && !IsFlatDamage;

        public bool AffectedByAttackingStat => !IsStatus && !IsOneHitKO && !IsFlatDamage && !IsCounterAttack;

        public bool IsOneHitKO => effect == MoveEffect.OneHitKill;

        public bool IsFlatDamage => effect is MoveEffect.FlatDamageLevel or MoveEffect.FlatDamage20 or MoveEffect.FlatDamage40 or MoveEffect.VaryingDamageLevel;

        public bool IsCounterAttack => effect is MoveEffect.Counter or MoveEffect.MirrorCoat or MoveEffect.MirrorMove or MoveEffect.Endeavor or MoveEffect.Bide or MoveEffect.MetalBurst;

        public bool IsSelfdestruct => effect == MoveEffect.Selfdestruct;

        public bool IsCallMove => effect is MoveEffect.Metronome or MoveEffect.NaturePower or MoveEffect.Assist or MoveEffect.CopyCat or MoveEffect.MeFirst or MoveEffect.MirrorMove;

        public bool IsSleepStatusMove => effect is MoveEffect.StatusSleep or MoveEffect.Yawn;

        public bool IsTrappingMove => effect is MoveEffect.Trap or MoveEffect.DoTTrap or MoveEffect.DoTTrapHitUnderwater2x;

        public bool IsHighCrit => effect is MoveEffect.DamageHighCrit or MoveEffect.DamageHighCritPoisonChance or MoveEffect.DamageHighCritBurnChance or MoveEffect.SkyAttack or MoveEffect.ChargeTurnHighCrit;

        public bool IsTwoTurnAttack => effect is MoveEffect.FutureAttack or MoveEffect.SkullBash or MoveEffect.ChargeTurn or MoveEffect.SkyAttack or MoveEffect.Solarbeam or MoveEffect.ChargeTurnHighCrit;

        public bool IsVeryLowAccuracy => accuracy > 0 && accuracy <= 50; // 0 accuracy moves always hit

        public int EffectivePower
        {
            get
            {
                if (IsTwoTurnAttack)
                {
                    return (int)Math.Floor(power * 0.75);
                }
                return effect switch
                {
                    MoveEffect.Multihit => power * 3,
                    MoveEffect.DamageTwoHit or MoveEffect.DamageTwoHitPoisonChance => power * 2,
                    MoveEffect.DamageThreeConsecutiveHits => 47,
                    MoveEffect.Selfdestruct => (int)Math.Floor(power / 2.5),
                    MoveEffect.Magnitude => 71,
                    MoveEffect.DamageTiredAfterUse => (int)Math.Floor(power / 1.5),
                    MoveEffect.DamageWeightBased => 40,
                    MoveEffect.FlatDamage20 => 50,
                    MoveEffect.FlatDamage40 => 70,
                    MoveEffect.FlatDamageLevel => 30,
                    MoveEffect.VaryingDamageLevel => 40,
                    MoveEffect.HiddenPower => 50,
                    MoveEffect.Present => 30,
                    MoveEffect.DamageMoreAtLowHP => 20,// Flail, reversal, etc
                    _ => power,
                };
            }
        }
        public bool HasUndefinedRealPower => power == 1;

        public string Description { get; set; }
        public int OrigininalDescriptionLength { get; set; }

        public bool IsStatus => MoveCategory == Type.Status;

        public Type MoveCategory
        {
            get
            {
                // Logic fog Gens I-III where move category is dependent on move type
                if(category == Type.TypeDependent)
                {
                    if (power <= 0)
                        return Type.Status;
                    if (move is Move.WEATHER_BALL)
                        return Type.Special;
                    if (move is Move.HIDDEN_POWER)
                        return Type.Unknown;
                    return type <= PokemonType.Unknown ? Type.Physical : Type.Special;
                }
                return category;
            }
            set
            {
                category = value;
            }
        }
        private Type category = Type.TypeDependent;

        public MoveData() { }

        public MoveData(MoveData toCopy)
        {
            effect = toCopy.effect;
            power = toCopy.power;
            type = toCopy.type;
            accuracy = toCopy.accuracy;
            pp = toCopy.pp;
            effectChance = toCopy.effectChance;
            targets = toCopy.targets;
            priority = toCopy.priority;
            flags = new BitArray(toCopy.flags);
            Description = toCopy.Description;
            OrigininalDescriptionLength = toCopy.OrigininalDescriptionLength;
        }

        public override string ToString()
        {
            return move.ToDisplayString();
        }
    }
}
