using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.EnumTypes
{
    public enum HeldItemEffect
    {
        None,
        HealFixedAtHalfHpOnce, // param is hp healed
        CureParalysisOnce,
        CureSleepOnce,
        CurePoisonOnce,
        CureBurnOnce,
        CureFreezeOnce,
        RestorePPWhenEmptyOnce, // param is PP restored
        CureConfusionOnce,
        CureAllStatusOnce,
        HealFractionMayConfuseAtkOnce, // param is divisor of fraction of HP healed
        HealFractionMayConfuseSpAtkOnce, // param is divisor of fraction of HP healed
        HealFractionMayConfuseSpdOnce, // param is divisor of fraction of HP healed
        HealFractionMayConfuseSpDefOnce, // param is divisor of fraction of HP healed
        HealFractionMayConfuseDefOnce, // param is divisor of fraction of HP healed
        BoostAtkOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        BoostDefOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        BoostSpdOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        BoostSpAtkOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        BoostSpDefOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        BoostCritChanceOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        SharplyBoostRandomStatOnce, // param is divisor of fraction of HP trigger (e.g 4 means trigger at 25% HP)
        ReduceEnemyAccuracy, // param is percent reduction
        CureStatLossOnce,
        MachoBrace,
        ExpShare,
        MayMoveFirst, // param is activation chance
        BoostFriendliness,
        CureInfatuationOnce, // TODO gen V - mental herb expands its effects
        ChoiceBand,
        EnemyMayFlinch, // param is activation chance
        BoostBugMoves, // param is %boost
        DoubleMoney,
        RepelWildPokemon,
        SoulDew,
        DeepSeaFang,
        DeepSeaScale,
        SmokeBall,
        Everstone,
        MayEndureHit, // param is activation chance
        LuckyEgg,
        BoostCritChance,
        BoostSteelMoves, // param is %boost
        Leftovers,
        Nop1,
        LightBall,
        BoostGroundMoves, // param is %boost
        BoostRockMoves, // param is %boost
        BoostGrassMoves, // param is %boost
        BoostDarkMoves, // param is %boost
        BoostFightingMoves, // param is %boost
        BoostElectricMoves, // param is %boost
        BoostWaterMoves, // param is %boost
        BoostFlyingMoves, // param is %boost
        BoostPoisonMoves, // param is %boost
        BoostIceMoves, // param is %boost
        BoostGhostMoves, // param is %boost
        BoostPsychicMoves, // param is %boost
        BoostFireMoves, // param is %boost
        BoostDragonMoves, // param is %boost
        BoostNormalMoves, // param is %boost
        Upgrade,
        ShellBell, // param is divisor of fraction of damage healed
        LuckyPunch,
        MetalPowder,
        ThickClub,
        Stick,
    }
}
