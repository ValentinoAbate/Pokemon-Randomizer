using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.DataStructures.MoveData;
using SpecialMoveSettings = PokemonRandomizer.Settings.SpecialMoveSettings;

namespace PokemonRandomizer.Backend.Randomization
{
    public class MovesetGenerator
    {
        private const float needSynergy = 12500;
        private const float preferSynergy = needSynergy / 2;
        private const float weakSynergy = needSynergy / 100;
        private const float strongAntiSynergy = needSynergy;
        private const float avoidAntiSynergy = preferSynergy;
        private const float weakAntiSynergy = weakSynergy;

        private readonly IDataTranslator dataT;
        private readonly Random rand;
        private readonly HashSet<Move> allValidMoves;

        public MovesetGenerator(HashSet<Move> allValidMoves, IDataTranslator dataT, Random rand)
        {
            this.dataT = dataT;
            this.rand = rand;
            this.allValidMoves = allValidMoves;
        }

        public static Move[] DefaultMoveset(PokemonBaseStats pokemon, int level)
        {
            var learnSet = pokemon.learnSet;
            var moves = new Stack<Move>(Math.Max(learnSet.Count, 4));
            foreach (var item in learnSet)
            {
                if (item.learnLvl > level)
                    break;
                moves.Push(item.move);
            }
            while (moves.Count < 4)
                moves.Push(Move.None);
            return new Move[] { moves.Pop(), moves.Pop(), moves.Pop(), moves.Pop() };
        }

        private static Move[] EmptyMoveset() => new Move[4] { Move.None, Move.None, Move.None, Move.None };

        private static float EffectivePower(MoveData data)
        {
            int power = data.power;
            if (data.IsTwoTurnAttack)
            {
                return (int)Math.Floor(power * 0.75);
            }
            return data.effect switch
            {
                MoveEffect.Multihit => power * 3,
                MoveEffect.DamageTwoHit or MoveEffect.DamageTwoHitPoisonChance => power * 2,
                MoveEffect.DamageThreeConsecutiveHits => 47,
                MoveEffect.Selfdestruct => (int)Math.Floor(power / 2.75),
                MoveEffect.Magnitude => 71,
                MoveEffect.DamageTiredAfterUse => (int)Math.Floor(power / 1.75),
                MoveEffect.DamageWeightBased => 40,
                MoveEffect.FlatDamage20 => 45,
                MoveEffect.FlatDamage40 => 65,
                MoveEffect.FlatDamageLevel => 30,
                MoveEffect.VaryingDamageLevel => 40,
                MoveEffect.HiddenPower => 50,
                MoveEffect.Present => 30,
                MoveEffect.DamageMoreAtLowHP => 20,// Flail, reversal, etc
                MoveEffect.FocusPunch => (int)Math.Floor(power / 1.75),
                MoveEffect.WeatherBall => power * 2,
                MoveEffect.DamageCannotKill => (int)Math.Floor(power * 0.25),
                _ => power,
            };
        }

        private float PowerFactorScale(Move m, PokemonBaseStats pokemon)
        {
            var data = dataT.GetMoveData(m);
            float basePower = data.IsCallMove ? 25 : EffectivePower(data);
            if (data.AffectedByStab && IsStab(data, pokemon))
            {
                basePower *= 1.5f;
            }
            basePower *= SignatureMoveFactor(data, pokemon);
            basePower *= AccuracyFactor(data, pokemon);
            basePower *= AttackingStatFactor(data, pokemon);
            return MathF.Pow(basePower, 3);
        }

        private static float SignatureMoveFactor(MoveData data, PokemonBaseStats pokemon)
        {
            if (data.move is Move.WEATHER_BALL && pokemon.species is Pokemon.CASTFORM)
            {
                return 2;
            }
            return 1;
        }

        private static float AccuracyFactor(MoveData data, PokemonBaseStats pokemon)
        {
            // Move ignores accuracy
            if (data.accuracy == 0)
                return 1;
            float accuracy = data.accuracy;
            // TODO: actually check if the pokemon currently being randomized has the ability
            // Compound Eyes (doesn't affect OHKO moves)
            if (pokemon.HasAbility(Ability.Compoundeyes) && !data.IsOneHitKO)
            {
                accuracy = MathF.Min(accuracy * 1.3f, 100);
            }
            // TODO: Hustle
            // TODO: No Guard       
            return accuracy / 100;
        }

        private static float AttackingStatFactor(MoveData data, PokemonBaseStats pokemon) 
        {
            if (!data.AffectedByAttackingStat)
                return 1;
            int attack = pokemon.EffectiveAttack;
            float maxStat = MathF.Max(attack, pokemon.SpAttack);
            if(data.MoveCategory == MoveData.Type.Special)
            {
                return pokemon.SpAttack / maxStat;
            }
            else if (data.MoveCategory == MoveData.Type.Physical)
            {
                return attack / maxStat;
            }
            else
            {
                return 1;
            }
        }

        private static float LevelWeightScale(int learnLevel)
        {
            return MathF.Pow(learnLevel, 2);
        }

        private static float LevelWeightScaleSmall(int learnLevel)
        {
            return MathF.Pow(learnLevel, 1.5f);
        }

        private static bool IsAttackMoveOfType(MoveData m, PokemonType t)
        {
            return !m.IsStatus && m.IsType(t);
        }

        public Move[] SmartMoveSet(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings = null)
        {
            if (pokemon.species is Pokemon.SHUCKLE or Pokemon.HAPPINY or Pokemon.CHANSEY or Pokemon.BLISSEY or Pokemon.HOPPIP or Pokemon.SKIPLOOM or Pokemon.JUMPLUFF)
            {
                return LowAttackMoveSet(pokemon, level, specialMoveSettings);
            }
            else if (pokemon.species is Pokemon.LILEEP or Pokemon.CRADILY)
            {
                return rand.RandomBool() ? LowAttackMoveSet(pokemon, level, specialMoveSettings) : AttackingMoveset(pokemon, level, specialMoveSettings);
            }
            else if (pokemon.species is Pokemon.WYNAUT or Pokemon.WOBBUFFET)
            {
                return CounterAttackMoveSet(pokemon, level, specialMoveSettings);
            }
            else if (pokemon.species is Pokemon.SMEARGLE)
            {
                int sketches = 0;
                foreach(var entry in pokemon.learnSet)
                {
                    if(entry.learnLvl > level)
                    {
                        break;
                    }
                    if(entry.move == Move.SKETCH)
                    {
                        ++sketches;
                    }
                }
                double smeargleSetChance = rand.RandomDouble();
                if (smeargleSetChance < 0.01)
                {
                    return CounterAttackMoveSet(pokemon, level, specialMoveSettings, sketches);
                }
                else if(smeargleSetChance < 0.02)
                {
                    return AttackingMoveset(pokemon, level, specialMoveSettings, sketches);
                }
                else if(smeargleSetChance < 0.07)
                {
                    return LowAttackMoveSet(pokemon, level, specialMoveSettings, sketches);
                }
                else
                {
                    return RandomMoveSet(pokemon, level, specialMoveSettings, sketches);
                }
            }
            else 
            {
                return AttackingMoveset(pokemon, level, specialMoveSettings);
            }
        }

        private static bool IsStab(MoveData move, PokemonBaseStats pokemon)
        {
            return pokemon.IsType(move.type);
        }

        public Move[] AttackingMoveset(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings, int maxMoves = 4)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level, specialMoveSettings);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;
            // Initalize synergy metrics
            var synergyMetrics = new List<Func<Move, float>>();
            var antiSynergyMetrics = new List<Func<Move, float>>();

            float PowerFactor(Move m) => PowerFactorScale(m, pokemon);
            float RedundantTypeFactor(Move m)
            {
                var mType = dataT.GetMoveData(m).type;
                foreach (var m2 in ret)
                {
                    if (m2 == Move.None)
                        continue;
                    if (mType == dataT.GetMoveData(m2).type)
                        return 0.25f;
                }
                return 1;
            }
            float StabBonus(Move m) => IsStab(dataT.GetMoveData(m), pokemon) ? 2f : 1;
            float LevelFactor(Move e) => MathF.Pow(availableMoves[e], 2);
            float LevelFactorSmall(Move e) => MathF.Pow(availableMoves[e], 1.5f);
            float LevelFactorLog(Move e) => MathF.Max(1, MathF.Log(availableMoves[e]));

            // Calculate Move Synergies
            CalculateMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);

            // Choose first move - attempt to choose an attack move
            if (ChooseMoveForIndex(ret, 0, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * LevelFactorLog(m) * MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics), ref availableMoves) || maxMoves <= 1)
            {
                return ret;
            }

            // Calculate Move Synergies
            synergyMetrics.Clear();
            antiSynergyMetrics.Clear();
            CalculateMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);

            // Choose second move - attempt to choose another attack move
            if (ChooseMoveForIndex(ret, 1, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * RedundantTypeFactor(m) * LevelFactorLog(m) * MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics), ref availableMoves) || maxMoves <= 2)
            {
                return ret;
            }

            // Calculate Move Synergies
            synergyMetrics.Clear();
            antiSynergyMetrics.Clear();
            CalculateMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);

            // Choose third move - Attempt to choose a status move
            if (ChooseMoveForIndex(ret, 2, GetStatusMoves(availableMoves), (m) => LevelFactorSmall(m) * MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics), ref availableMoves) || maxMoves <= 3)
            {
                return ret;
            }

            // Calculate Move Synergies
            synergyMetrics.Clear();
            antiSynergyMetrics.Clear();
            CalculateFinalMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);

            // Choose fourth move
            ret[3] = rand.Choice(new WeightedSet<Move>(availableMoves.Keys, m => LevelFactor(m) * MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics)));

            //Logger.main.Info($"{pokemon.species.ToDisplayString()} LV {level}: {ret[0].ToDisplayString()}, {ret[1].ToDisplayString()}, {ret[2].ToDisplayString()}, {ret[3].ToDisplayString()}");
            return ret;
        }

        private static float MoveSynergyFactor(Move m, List<Func<Move, float>> synergyMetrics, List<Func<Move, float>> antiSynergyMetrics)
        {
            float amount = 1;
            if(synergyMetrics.Count > 0)
            {
                float sum = 0;
                foreach (var metric in synergyMetrics)
                {
                    sum += metric(m);
                }
                if(sum > 0)
                {
                    amount *= sum;
                }
            }
            if(antiSynergyMetrics.Count > 0)
            {
                float sum = 0;
                foreach(var metric in antiSynergyMetrics)
                {
                    sum += metric(m);
                }
                if (sum > 0)
                {
                    amount /= sum;
                }
            }
            return amount;
        }

        private void CalculateFinalMoveSynergyMetrics(Move[] currentMoves, IReadOnlyDictionary<Move, int> moveChoices, List<Func<Move, float>> synergyMetrics, List<Func<Move, float>> antiSynergyMetrics)
        {
            var currentMovesProcessed = currentMoves.Where(m => m != Move.None).Select(dataT.GetMoveData);
            void CalculateMoveSynergy(Func<MoveData, bool> currMovePred, Func<MoveData, bool> moveChoicePred, float intensity, bool antiSynergy = false, bool applyMultiple = false)
            {
                int count = currentMovesProcessed.Count(currMovePred);
                // If we don't have any moves that need this synergy, return
                if (count <= 0)
                    return;
                // Synergy is already fulfilled, return
                if (!applyMultiple && currentMovesProcessed.Any(moveChoicePred))
                    return;
                var metricList = antiSynergy ? antiSynergyMetrics : synergyMetrics;
                metricList.Add(m => moveChoicePred(dataT.GetMoveData(m)) ? intensity * count : 0);
            }

            void MissingMoveSynergy(Func<MoveData, bool> currMovePred, Func<MoveData, bool> moveChoicePred, float intensity, bool antiSynergy = false) 
            {
                // If we already have the necessary combo move, return
                if (currentMovesProcessed.Any(currMovePred))
                    return;
                if (!moveChoices.Any(kvp => moveChoicePred(dataT.GetMoveData(kvp.Key))))
                    return;
                var metricList = antiSynergy ? antiSynergyMetrics : synergyMetrics;
                metricList.Add(m => moveChoicePred(dataT.GetMoveData(m)) ? intensity : 0);
            }

            // Move Synergies

            // Nightmare or Dream Eater + Sleep move Synergy
            CalculateMoveSynergy(RequiresSleep, CausesSleep, needSynergy);
            // Snore or Sleep Talk + Rest Synergy
            CalculateMoveSynergy(RequiresRest, IsRest, needSynergy);
            // Rollout or Ice Ball + Defense Curl Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.MultiTurnBuildup, m => m.effect == MoveEffect.DefPlus1AndPrepForRoll, preferSynergy);
            // Spit Up or Swallow + Stockpile Synergy
            CalculateMoveSynergy(RequiresStockpile, IsStockpile, needSynergy);
            // Stockpile + Spit Up or Swallow Synergy
            CalculateMoveSynergy(IsStockpile, RequiresStockpile, preferSynergy, false, true);
            // Sun Move + Sun
            CalculateMoveSynergy(m => m.effect is MoveEffect.Solarbeam or MoveEffect.RecoverHpWeather1 or MoveEffect.RecoverHpWeather2 or MoveEffect.RecoverHpWeather3, m => m.effect == MoveEffect.WeatherSun, preferSynergy);
            // Attacking Fire Move + Sun
            CalculateMoveSynergy(m => IsAttackMoveOfType(m, PokemonType.FIR), m => m.effect == MoveEffect.WeatherSun, weakSynergy);
            // Rain move + Rain
            CalculateMoveSynergy(m => m.effect == MoveEffect.Thunder, m => m.effect == MoveEffect.WeatherRain, preferSynergy);
            // Attacking Water Move + Rain
            CalculateMoveSynergy(m => IsAttackMoveOfType(m, PokemonType.WAT), m => m.effect == MoveEffect.WeatherRain, weakSynergy);
            // Weather Ball + Weather (Rain / Sun / Hail)
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall, m => m.effect is MoveEffect.WeatherRain or MoveEffect.WeatherSun or MoveEffect.WeatherHail, needSynergy, false, true);
            // Weather Ball + Sandstorm
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall, m => m.effect == MoveEffect.WeatherSandstorm, weakSynergy);
            // Charge + Attacking Electric Move
            CalculateMoveSynergy(m => m.effect == MoveEffect.Charge, m => IsAttackMoveOfType(m, PokemonType.ELE), preferSynergy);
            // Endure + Endeavor
            CalculateMoveSynergy(m => m.effect == MoveEffect.Endure, m => m.effect == MoveEffect.Endeavor, preferSynergy);
            // Endure + Flailing Move
            CalculateMoveSynergy(m => m.effect == MoveEffect.Endure, m => m.effect == MoveEffect.DamageMoreAtLowHP, preferSynergy);
            // Flailing Move + Endure
            CalculateMoveSynergy(m => m.effect == MoveEffect.DamageMoreAtLowHP, m => m.effect == MoveEffect.Endure, weakSynergy);
            // Perish song + Trapping move
            CalculateMoveSynergy(m => m.effect is MoveEffect.PerishSong, m => m.IsTrappingMove, preferSynergy);
            // Lock-on + OHKO or low acc move
            CalculateMoveSynergy(m => m.effect is MoveEffect.NextMoveAlwaysHits, m => m.IsVeryLowAccuracy, preferSynergy);
            // OHKO or low acc move + Lock-on
            CalculateMoveSynergy(m => m.IsVeryLowAccuracy, m => m.effect is MoveEffect.NextMoveAlwaysHits, preferSynergy);
            // Focus Energy + high crit move
            CalculateMoveSynergy(m => m.effect is MoveEffect.StatusCritRateUp, m => m.IsHighCrit, preferSynergy);

            // Move Anti-Synergies

            // Stockpile w/out Spit Up or Swallow Antisynergy TODO: Gen IV - lift this restriction or set to weak (stockpile has a standalone effect)
            MissingMoveSynergy(RequiresStockpile, IsStockpile, strongAntiSynergy, true);
            // Spit Up or Swallow w/out Stockpile Antisynergy
            MissingMoveSynergy(IsStockpile, RequiresStockpile, strongAntiSynergy, true);
            // Lock-on w/out OHKO or low acc move Antisynergy
            MissingMoveSynergy(m => m.IsVeryLowAccuracy, m => m.effect is MoveEffect.NextMoveAlwaysHits, avoidAntiSynergy, true);
            // Charge w/out Attacking Electric Move Antisynergy TODO: Gen IV - lift this restiction or set to weak (charge has a standalone effect)
            MissingMoveSynergy(m => IsAttackMoveOfType(m, PokemonType.ELE), m => m.effect == MoveEffect.Charge, strongAntiSynergy, true);
            // Nightmare or Dream Eater w/out Sleep move Antisynergy
            MissingMoveSynergy(CausesSleep, RequiresSleep, strongAntiSynergy, true);
        }
        
        private void CalculateMoveSynergyMetrics(Move[] currentMoves, IReadOnlyDictionary<Move, int> moveChoices, List<Func<Move, float>> synergyMetrics, List<Func<Move, float>> antiSynergyMetrics)
        {
            void MissingMoveSynergy(Func<MoveData, bool> moveChoicePred, Func<MoveData, bool> requiredMovePred, float intensity, bool antiSynergy = false)
            {
                // If we already know the required move, return
                foreach (var move in currentMoves)
                {
                    if (requiredMovePred(dataT.GetMoveData(move)))
                        return;
                }
                // If we can choose the required move, return
                foreach (var kvp in moveChoices) 
                {
                    if (requiredMovePred(dataT.GetMoveData(kvp.Key)))
                        return;
                }
                var metricList = antiSynergy ? antiSynergyMetrics : synergyMetrics;
                metricList.Add(m => moveChoicePred(dataT.GetMoveData(m)) ? intensity : 0);
            }
            // Dream Eater or Nightmare but can't choose sleep move antisynergy
            MissingMoveSynergy(RequiresSleep, CausesSleep, strongAntiSynergy, true);
            // Spit up or Swallow but can't choose stockpile
            MissingMoveSynergy(RequiresStockpile, IsStockpile, strongAntiSynergy, true);
            // Stockpile but can't choose spit up or swallow antisynergergy TODO: Gen IV - lift this restriction
            MissingMoveSynergy(IsStockpile, RequiresStockpile, strongAntiSynergy, true);
        }

        private static bool RequiresSleep(MoveData m) => m.effect is MoveEffect.DreamEater or MoveEffect.StatusNightmare;
        private static bool CausesSleep(MoveData m) => m.IsSleepStatusMove;
        private static bool RequiresRest(MoveData m) => m.effect is MoveEffect.SleepTalk or MoveEffect.DamageFailUnlessAsleepFlinchChance;
        private static bool IsRest(MoveData m) => m.effect is MoveEffect.Rest;
        private static bool RequiresStockpile(MoveData m) => m.effect is MoveEffect.Swallow or MoveEffect.SpitUp;
        private static bool IsStockpile(MoveData m) => m.effect is MoveEffect.Stockpile;


        public Move[] LowAttackMoveSet(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings, int maxMoves = 4)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level, specialMoveSettings);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var availableMovesKeys = availableMoves.Keys;

            // Choose first move - attempt to choose a damaging status move or DOT move. Favor same type
            var preferredMoves = new WeightedSet<Move>(availableMoves.Count);
            preferredMoves.AddRange(availableMovesKeys, m => LowAttackMoveWeight(pokemon, dataT.GetMoveData(m), availableMoves.Count));
            if (ChooseMoveForIndex(ret, 0, preferredMoves, ref availableMoves) || maxMoves <= 1)
            {
                return ret;
            }

            // Choose second move - attempt to choose a buff / disruption move
            preferredMoves.Clear();
            preferredMoves.AddRange(availableMovesKeys, m => DefensiveMoveWeight(pokemon, dataT.GetMoveData(m)));
            var firstMoveData = dataT.GetMoveData(ret[0]);

            // Don't stack non-volatile status conditions or damaging weathers
            if(firstMoveData.effect is MoveEffect.StatusPoison or MoveEffect.StatusBurn or MoveEffect.StatusBadlyPoison)
            {
                preferredMoves.RemoveWhere(m => dataT.GetMoveData(m).effect is MoveEffect.StatusPoison or MoveEffect.StatusBurn or MoveEffect.StatusBadlyPoison);
            }
            else if (firstMoveData.effect is MoveEffect.WeatherSandstorm or MoveEffect.WeatherHail)
            {
                // Don't stack weather conditions
                preferredMoves.RemoveWhere(m => dataT.GetMoveData(m).effect is MoveEffect.WeatherSandstorm or MoveEffect.WeatherHail);
            }

            // Choose move
            if (ChooseMoveForIndex(ret, 1, preferredMoves, ref availableMoves) || maxMoves <= 2)
            {
                return ret;
            }

            
            // Choose third move: level-scaled status
            preferredMoves.Clear();
            preferredMoves.AddRange(GetStatusMoves(availableMoves));
            preferredMoves.Multiply(m => LevelWeightScaleSmall(availableMoves[m]));
            if (ChooseMoveForIndex(ret, 2, preferredMoves, ref availableMoves) || maxMoves <= 3)
            {
                return ret;
            }

            // Fourth move
            preferredMoves.Clear();
            preferredMoves.AddRange(availableMovesKeys);
            preferredMoves.Multiply(m => LevelWeightScale(availableMoves[m]));
            // Calculate Move Synergies
            var synergyMetrics = new List<Func<Move, float>>();
            var antiSynergyMetrics = new List<Func<Move, float>>();
            CalculateFinalMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);
            preferredMoves.Multiply(m => MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics));
            if (firstMoveData.effect is MoveEffect.StatusNightmare)
            {
                // If nightmare and we don't already have a sleep move, ensure sleep
                if(!ret.Any(m => m != Move.None && dataT.GetMoveData(m).IsSleepStatusMove))
                {
                    preferredMoves.RemoveWhere(m => !dataT.GetMoveData(m).IsSleepStatusMove);
                }

            }
            else if (firstMoveData.effect is MoveEffect.PerishSong && !(pokemon.HasAbility(Ability.Shadow_Tag) || pokemon.HasAbility(Ability.Arena_Trap)))
            {
                // If perish song, ensure mean look / block / DOT trap if possible (unless the pokemon already has shadow tag or arena trap)
                preferredMoves.RemoveWhere(m => !dataT.GetMoveData(m).IsTrappingMove);
                if(preferredMoves.Count <= 0)
                {
                    preferredMoves.AddRange(availableMovesKeys);
                    preferredMoves.Multiply(m => LevelWeightScale(availableMoves[m]));
                }
            }
            ChooseMoveForIndex(ret, 3, preferredMoves, ref availableMoves);
            return ret;
        }

        public Move[] CounterAttackMoveSet(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings, int maxMoves = 4)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level, specialMoveSettings);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var preferredMoves = new WeightedSet<Move>(availableMoves.Count);
            preferredMoves.AddRange(availableMoves.Keys.Where(m => dataT.GetMoveData(m).IsCounterAttack || dataT.GetMoveData(m).effect is MoveEffect.DestinyBond));
            preferredMoves.Multiply(m => LevelWeightScaleSmall(availableMoves[m]));
            // Todo: prefer STAB
            if (ChooseMoveForIndex(ret, 0, preferredMoves, ref availableMoves) || maxMoves <= 1)
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[0]);
            if (ChooseMoveForIndex(ret, 1, preferredMoves, ref availableMoves) || maxMoves <= 2)
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[1]);
            if (ChooseMoveForIndex(ret, 2, preferredMoves, ref availableMoves) || maxMoves <= 3)
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[2]);

            ChooseMoveForIndex(ret, 3, preferredMoves, ref availableMoves);
            return ret;
        }

        public Move[] RandomMoveSet(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings, int maxMoves = 4)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level, specialMoveSettings);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var preferredMoves = new WeightedSet<Move>(availableMoves.Keys);
            int moveIndex = 0;
            for(; moveIndex < 3 && moveIndex < (maxMoves - 1); ++moveIndex)
            {
                if (ChooseMoveForIndex(ret, moveIndex, preferredMoves, ref availableMoves))
                {
                    return ret;
                }
                preferredMoves.RemoveIfContains(ret[moveIndex]);
            }
            // Calculate Move Synergies
            var synergyMetrics = new List<Func<Move, float>>();
            var antiSynergyMetrics = new List<Func<Move, float>>();
            CalculateFinalMoveSynergyMetrics(ret, availableMoves, synergyMetrics, antiSynergyMetrics);
            preferredMoves.Multiply(m => MoveSynergyFactor(m, synergyMetrics, antiSynergyMetrics));
            // Choose final move
            ChooseMoveForIndex(ret, moveIndex, preferredMoves, ref availableMoves);
            return ret;
        }

        private const float lowAttackingStatPowerModifier = 0.25f;
        // Returns a value weight that approximates moves "power" for pokemon that have attacking stats so low that move power is irrelevant
        private float LowAttackMoveWeight(PokemonBaseStats pokemon, MoveData moveData, int choiceCount)
        {
            float weight = 0;
            // DoT Status
            if (moveData.effect is MoveEffect.StatusBadlyPoison or MoveEffect.StatusLeechSeed)
            {
                weight = 250;
            }
            else if (moveData.effect is MoveEffect.StatusBurn or MoveEffect.StatusPoison)
            {
                weight = 100;
            }
            // Flat damage
            else if (moveData.effect is MoveEffect.FlatDamageLevel)
            {
                weight = 200;
            }
            else if (moveData.effect is MoveEffect.VaryingDamageLevel)
            {
                weight = 120;
            }
            else if (moveData.effect is MoveEffect.FlatDamage40)
            {
                weight = 80;
            }
            else if (moveData.effect is MoveEffect.FlatDamage20)
            {
                weight = 40;
            }
            else if (moveData.effect is MoveEffect.HalfHp)
            {
                weight = 50;
            }
            // Volatile potentially damaging status
            else if (moveData.effect is MoveEffect.StatusConfuse or MoveEffect.StatusConfuseAll or MoveEffect.StatusConfuseAtkPlus2 or MoveEffect.StatusConfuseSpAtkPlus2)
            {
                weight = 10;
            }
            // Perish Song
            else if (moveData.effect is MoveEffect.PerishSong) // Perish trap route
            {
                if (pokemon.HasAbility(Ability.Shadow_Tag) || pokemon.HasAbility(Ability.Arena_Trap))
                {
                    weight = 250;
                }
                else if (pokemon.learnSet.Any(e => dataT.GetMoveData(e.move).IsTrappingMove))
                {
                    weight = 200;
                }
                else if (pokemon.HasAbility(Ability.Magnet_Pull))
                {
                    weight = 100;
                }
                else
                {
                    weight = 50;
                }
            }
            // Nightmare
            else if (moveData.effect is MoveEffect.StatusNightmare)
            {
                foreach (var entry in pokemon.learnSet)
                {
                    if (dataT.GetMoveData(entry.move).IsSleepStatusMove)
                    {
                        weight = 125;
                        break;
                    }
                }
            }
            // GHOST Curse
            else if (moveData.effect is MoveEffect.Curse)
            {
                if (pokemon.IsType(PokemonType.GHO))
                {
                    weight = 100;
                }
            }
            // DoT weather
            else if (moveData.effect is MoveEffect.WeatherSandstorm)
            {
                if (pokemon.IsType(PokemonType.RCK) || pokemon.IsType(PokemonType.GRD) || pokemon.IsType(PokemonType.STL))
                {
                    weight = 50;
                }
            }
            else if (moveData.effect is MoveEffect.WeatherHail)
            {
                if (pokemon.IsType(PokemonType.ICE))
                {
                    weight = 50;
                }
            }
            // Attacking moves
            else if (EffectivePower(moveData) > 0) // Normal Attacking Moves
            { 
                weight = (EffectivePower(moveData) * lowAttackingStatPowerModifier) / Math.Max(choiceCount * 0.1f, 1f);
                if (moveData.effect is MoveEffect.DoTTrap)
                {
                    weight += 75; 
                }
            }
            if (moveData.IsType(pokemon))
            {
                if (moveData.AffectedByStab && moveData.effect is not MoveEffect.DoTTrap)
                {
                    weight *= 1.5f;
                }
                else if (pokemon.IsType(PokemonType.NRM))
                {
                    if(moveData.effect is MoveEffect.DoTTrap)
                    {
                        weight *= 1.5f;
                    }
                }
                else
                {
                    weight *= 5;
                }
            }
            return weight;
        }

        private float DefensiveMoveWeight(PokemonBaseStats pokemon, MoveData moveData)
        {
            float weight = 0;
            // Healing moves
            if (moveData.effect is MoveEffect.RecoverHp or MoveEffect.RecoverHpWeather1 or MoveEffect.RecoverHpWeather2 or MoveEffect.RecoverHpWeather3 or MoveEffect.Softboiled)
            {
                weight = 250;
            }
            else if (moveData.effect is MoveEffect.Rest)
            {
                weight = 250;
            }
            else if (moveData.effect is MoveEffect.Wish)
            {
                weight = 200;
            }
            else if (moveData.effect is MoveEffect.StatusLeechSeed)
            {
                weight = 200;
            }
            else if (moveData.effect is MoveEffect.Ingrain)
            {
                weight = 100;
            }
            else if (moveData.effect is MoveEffect.PainSplit)
            {
                weight = 100;
            }
            // Status healing moves
            else if (moveData.effect is MoveEffect.CurePartyStatus)
            {
                weight = 100;
            }
            else if (moveData.effect is MoveEffect.CureStatus)
            {
                weight = 75;
            }
            // Stall moves
            else if (moveData.effect is MoveEffect.Protect)
            {
                weight = 75;
            }
            else if (moveData.effect is MoveEffect.Substitute)
            {
                weight = 75;
            }
            else if (moveData.effect is MoveEffect.Endure)
            {
                weight = 25;
            }
            // Defense / evasion buff moves
            else if (moveData.effect is MoveEffect.EvadePlus1)
            {
                weight = 100;
            }
            else if (moveData.effect is MoveEffect.EvadePlus1)
            {
                weight = 75;
            }
            else if (moveData.effect is MoveEffect.EvadePlus1AndVulnerable)
            {
                weight = 60;
            }
            else if (moveData.effect is MoveEffect.DefPlus2 or MoveEffect.SpDefPlus2 or MoveEffect.DefSpDefPlus1)
            {
                weight = 50;
            }
            else if (moveData.effect is MoveEffect.DefPlus1 or MoveEffect.DefPlus1AndPrepForRoll or MoveEffect.SpDefPlus1 or MoveEffect.AtkDefPlus1 or MoveEffect.SpAtkSpDefPlus1)
            {
                weight = 25;
            }
            else if (moveData.effect is MoveEffect.Curse)
            {
                if (pokemon.IsType(PokemonType.GHO))
                {
                    weight = 10;
                }
                else
                {
                    weight = 25;
                }
            }
            // Disruption
            else if (moveData.effect is MoveEffect.Encore)
            {
                weight = 25;
            }
            else if (moveData.IsSleepStatusMove || moveData.effect is MoveEffect.StatusParalyze or MoveEffect.StatusBurn or MoveEffect.StatusConfuse or MoveEffect.StatusConfuseAll or MoveEffect.StatusConfuseAtkPlus2 or MoveEffect.StatusConfuseSpAtkPlus2 or MoveEffect.Attract)
            {
                weight = 75;
            }
            else if (moveData.effect is MoveEffect.AccMinus1 or MoveEffect.AccMinus2 || moveData.move is Move.MUDーSLAP)
            {
                weight = 50;
            }
            else if (moveData.effect is MoveEffect.AtkMinus2 or MoveEffect.SpAtkMinus2)
            {
                weight = 50;
            }
            else if (moveData.effect is MoveEffect.AtkDefMinus1)
            {
                weight = 35;
            }
            else if (moveData.effect is MoveEffect.AtkMinus1 or MoveEffect.SpAtkMinus1)
            {
                weight = 25;
            }
            else if (moveData.effect is MoveEffect.Disable or MoveEffect.Taunt or MoveEffect.Torment)
            {
                weight = 50;
            }
            else if (moveData.effect is MoveEffect.Spite)
            {
                weight = 25;
            }
            else if (moveData.effect is MoveEffect.Snatch or MoveEffect.Trick)
            {
                weight = 10;
            }
            // Hazards
            else if (moveData.effect is MoveEffect.Spikes)
            {
                weight = 50;
            }
            // Screens, etc
            else if (moveData.effect is MoveEffect.LightScreen or MoveEffect.Reflect)
            {
                weight = 50;
            }
            else if (moveData.effect is MoveEffect.Safeguard or MoveEffect.Mist)
            {
                weight = 25;
            }
            // Random
            else if (moveData.effect is MoveEffect.Mimic or MoveEffect.MirrorMove or MoveEffect.MagicCoat or MoveEffect.Counter or MoveEffect.MirrorCoat)
            {
                weight = 15;
            }
            else if (moveData.effect is MoveEffect.Metronome or MoveEffect.Assist)
            {
                weight = 10;
            }
            // Destiny bond or Grudge
            else if (moveData.effect is MoveEffect.DestinyBond)
            {
                weight = 25;
            }
            else if (moveData.effect is MoveEffect.Grudge)
            {
                weight = 15;
            }

            if (moveData.IsType(pokemon) && !pokemon.IsType(PokemonType.NRM))
            {
                weight *= 5;
            }
            return weight;
        }

        private bool ChooseMoveForIndex(Move[] moveset, int index, IEnumerable<Move> preferredMoves, Func<Move, float> moveWeight, ref Dictionary<Move, int> availableMoves)
        {
            return ChooseMoveForIndex(moveset, index, new WeightedSet<Move>(preferredMoves, moveWeight), ref availableMoves);
        }

        private bool ChooseMoveForIndex(Move[] moveset, int index, WeightedSet<Move> preferredMoves, ref Dictionary<Move, int> availableMoves)
        {
            moveset[index] = ChooseMove(preferredMoves, availableMoves);
            availableMoves.Remove(moveset[index]);
            return availableMoves.Count <= 0;
        }

        private Move ChooseMove(WeightedSet<Move> preferredMoves, Dictionary<Move, int> allMoves)
        {
            // Preferred strategry
            if (preferredMoves.Count > 0)
            {
                return rand.Choice(preferredMoves);
            }
            // Fallback strategy
            var fallbackMoves = new WeightedSet<Move>(allMoves.Count);
            foreach (var kvp in allMoves)
            {
                fallbackMoves.Add(kvp.Key, LevelWeightScale(kvp.Value));
            }
            return rand.Choice(fallbackMoves);
        }

        private IEnumerable<Move> GetAttackMoves(Dictionary<Move, int> moves)
        {
            return moves.Keys.Where(CountsAsAttackingMove);
        }

        private bool CountsAsAttackingMove(Move m)
        {
            var data = dataT.GetMoveData(m);
            return !data.IsStatus || data.IsCallMove;
        }

        private IEnumerable<Move> GetStatusMoves(Dictionary<Move, int> moves)
        {
            return moves.Keys.Where(IsValidStatusMove);
        }

        private bool IsValidStatusMove(Move m)
        {
            if (m == Move.SPLASH)
                return false;
            return dataT.GetMoveData(m).IsStatus;
        }

        private Dictionary<Move, int> AvailableMoves(PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings)
        {
            // Give smeargle access to all moves
            if(pokemon.species is Pokemon.SMEARGLE)
            {
                // TODO: Gen-specific sketch logic (some moves are unsketchable)
                var moves = new Dictionary<Move, int>(allValidMoves.Count);
                foreach(var move in allValidMoves)
                {
                    moves.Add(move, 1);
                }
                if (moves.ContainsKey(Move.SKETCH))
                {
                    moves.Remove(Move.SKETCH);
                }
                return moves;
            }
            var availableMoves = new Dictionary<Move, int>(pokemon.learnSet.Count * 4);
            AddMovesFromPokemon(ref availableMoves, pokemon, level);
            AddSpecialMoves(ref availableMoves, pokemon, level, specialMoveSettings);
            if(pokemon.evolvesFrom.Count > 0)
            {
                // Create moveset lookup
                var movesetLookup = pokemon.learnSet.GetMovesLookup();                
                // Add movesets from evolved from (moves down the chain)
                for(var preEvolution = pokemon; preEvolution.evolvesFrom.Count > 0;)
                {
                    preEvolution = dataT.GetBaseStats(preEvolution.evolvesFrom[0].Pokemon);
                    AddMovesFromPreEvolution(ref availableMoves, movesetLookup, level, preEvolution);
                } 
            }
            return availableMoves;
        }

        private void AddMovesFromPokemon(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level)
        {
            foreach (var entry in pokemon.learnSet)
            {
                if(entry.learnLvl > level)
                {
                    break;
                }
                AddMove(ref availableMoves, entry);
            }
        }

        private void AddMovesFromPreEvolution(ref Dictionary<Move, int> availableMoves, HashSet<Move> pokemonMovesetLookup, int level, PokemonBaseStats preEvolution)
        {
            foreach(var entry in preEvolution.learnSet)
            {
                // Ignore moves past the learn level and SPLASH
                if(entry.learnLvl > level || entry.move == Move.SPLASH)
                {
                    continue;
                }
                else if (availableMoves.ContainsKey(entry.move))
                {
                    if(entry.learnLvl > availableMoves[entry.move])
                    {
                        // Move is higher level, update learn level data
                        availableMoves[entry.move] = entry.learnLvl;
                    }
                    continue;
                }
                else if (!pokemonMovesetLookup.Contains(entry.move))
                {
                    // Pre-evo exclusive move, add to available moves
                    AddMove(ref availableMoves, entry);
                }
            }
        }

        private void AddSpecialMoves(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level, SpecialMoveSettings specialMoveSettings)
        {
            if (specialMoveSettings == null)
                return;
            if (specialMoveSettings.AllowedSources.HasFlag(SpecialMoveSettings.Sources.Egg))
            {
                foreach(var move in pokemon.eggMoves)
                {
                    AddMove(ref availableMoves, move, 1);
                }
            }
            if (specialMoveSettings.AllowedSources.HasFlag(SpecialMoveSettings.Sources.TM))
            {
                for (int i = 0; i< pokemon.TMCompat.Count; ++i)
                {
                    if (pokemon.TMCompat[i])
                    {
                        AddMove(ref availableMoves, dataT.GetTmMove(i), 1);
                    }
                }
            }
            if (specialMoveSettings.AllowedSources.HasFlag(SpecialMoveSettings.Sources.HM))
            {
                for (int i = 0; i < pokemon.HMCompat.Count; ++i)
                {
                    if (pokemon.HMCompat[i])
                    {
                        AddMove(ref availableMoves, dataT.GetHmMove(i), 1);
                    }
                }
            }
            if (specialMoveSettings.AllowedSources.HasFlag(SpecialMoveSettings.Sources.Tutor))
            {
                for (int i = 0; i < pokemon.moveTutorCompat.Count; ++i)
                {
                    if (pokemon.moveTutorCompat[i])
                    {
                        AddMove(ref availableMoves, dataT.GetTutorMove(i), 1);
                    }
                }
            }
        }

        private void AddMove(ref Dictionary<Move, int> availableMoves, LearnSet.Entry entry) => AddMove(ref availableMoves, entry.move, entry.learnLvl);

        private void AddMove(ref Dictionary<Move, int> availableMoves, Move move, int learnLevel)
        {
            if (!availableMoves.ContainsKey(move))
            {
                availableMoves.Add(move, learnLevel);
            }
            else if (availableMoves[move] < learnLevel)
            {
                availableMoves[move] = learnLevel;
            }
        }

        private const float weakMod = 1.25f;
        private const float strongMod = 2f;
        private Func<Move, float> GetAbilityModifier(Ability ability)
        {
            Func<Move, float> AbilityMod(Predicate<MoveData> moveChoicePred, float intensity)
            {
                return m => moveChoicePred(dataT.GetMoveData(m)) ? intensity : 1;
            }
            switch (ability)
            {
                case Ability.Chlorophyll or Ability.Leaf_Guard or Ability.Solar_Power:
                    return AbilityMod(m => m.effect is MoveEffect.WeatherSun, weakMod);
                case Ability.Flower_Gift:
                    return AbilityMod(m => m.effect is MoveEffect.WeatherSun, strongMod);
                case Ability.Swift_Swim or Ability.Rain_Dish:
                    return AbilityMod(m => m.effect is MoveEffect.WeatherRain, weakMod);
                case Ability.Sand_Veil:
                    return AbilityMod(m => m.effect is MoveEffect.WeatherSandstorm, weakMod);
                case Ability.Ice_Body or Ability.Snow_Cloak:
                    return AbilityMod(m => m.effect is MoveEffect.WeatherHail, weakMod);
                // Secondary effect move + serene grace
                // Rock head + recoil moves
                default:
                    return m => 1;
            };
        }
    }
}
