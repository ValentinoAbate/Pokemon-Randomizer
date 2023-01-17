﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.DataStructures.MoveData;

namespace PokemonRandomizer.Backend.Randomization
{
    public class MovesetGenerator
    {
        private const float needSynergy = 12500;
        private const float preferSynergy = needSynergy / 2;
        private const float weakSynergy = needSynergy / 10;

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

        private float PowerFactorScale(Move m, bool isStab)
        {
            var data = dataT.GetMoveData(m);
            float basePower = data.IsCallMove ? 25 : data.EffectivePower;
            if (isStab)
            {
                basePower *= 1.5f;
            }
            return MathF.Pow(basePower, 3);
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

        public Move[] SmartMoveSet(PokemonBaseStats pokemon, int level)
        {
            if (pokemon.species is Pokemon.SHUCKLE or Pokemon.HAPPINY or Pokemon.CHANSEY or Pokemon.BLISSEY or Pokemon.HOPPIP or Pokemon.SKIPLOOM or Pokemon.JUMPLUFF)
            {
                return LowAttackMoveSet(pokemon, level);
            }
            else if (pokemon.species is Pokemon.LILEEP or Pokemon.CRADILY)
            {
                return rand.RandomBool() ? LowAttackMoveSet(pokemon, level) : AttackingMoveset(pokemon, level);
            }
            else if (pokemon.species is Pokemon.WYNAUT or Pokemon.WOBBUFFET)
            {
                return CounterAttackMoveSet(pokemon, level);
            }
            else if (pokemon.species is Pokemon.SMEARGLE)
            {
                double smeargleSetChance = rand.RandomDouble();
                // TODO: limit smeargle's number of moves depending on the number of times it could have learned sketch
                if (smeargleSetChance < 0.01)
                {
                    return CounterAttackMoveSet(pokemon, level);
                }
                else if(smeargleSetChance < 0.02)
                {
                    return AttackingMoveset(pokemon, level);
                }
                else if(smeargleSetChance < 0.07)
                {
                    return LowAttackMoveSet(pokemon, level);
                }
                else
                {
                    return RandomMoveSet(pokemon, level);
                }
            }
            else 
            {
                return AttackingMoveset(pokemon, level);
            }
        }

        public Move[] AttackingMoveset(PokemonBaseStats pokemon, int level)
        {
            bool IsStab(Move m) => pokemon.IsType(dataT.GetMoveData(m).type);
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;
            float PowerFactor(Move m) => PowerFactorScale(m, IsStab(m));
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
            float StabBonus(Move m) => IsStab(m) ? 2f : 1;
            float LevelFactor(Move e) => MathF.Pow(availableMoves[e], 2);
            float LevelFactorSmall(Move e) => MathF.Pow(availableMoves[e], 1.5f);
            float LevelFactorLog(Move e) => MathF.Max(1, MathF.Log(availableMoves[e]));

            // Choose first move - attempt to choose an attack move
            if (ChooseMoveForIndex(ret, 0, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * LevelFactorLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose second move - attempt to choose another attack move
            if (ChooseMoveForIndex(ret, 1, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * RedundantTypeFactor(m) * LevelFactorLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose third move - Attempt to choose a status move
            if (ChooseMoveForIndex(ret, 2, GetStatusMoves(availableMoves), LevelFactorSmall, ref availableMoves))
            {
                return ret;
            }

            var fourthMoveChoice = new WeightedSet<Move>(availableMoves.Keys, LevelFactor);
            var currentMoves = ret.Where(m => m != Move.None).Select(dataT.GetMoveData);


            // Apply synergy metrics

            // Calculate Move Synergies
            var metrics = CalculateMoveSynergyMetrics(ret);

            // Choose fourth move
            ret[3] = rand.Choice(new WeightedSet<Move>(availableMoves.Keys, m => LevelFactor(m) * MoveSynergyFactor(metrics, m)));

            return ret;
        }

        private float MoveSynergyFactor(List<Func<Move, float>> metrics, Move m) => Math.Max(1, metrics.Sum((metric) => metric(m)));

        private List<Func<Move, float>> CalculateMoveSynergyMetrics(Move[] currentMoves)
        {
            var currentMovesProcessed = currentMoves.Where(m => m != Move.None).Select(dataT.GetMoveData);
            var metrics = new List<Func<Move, float>>();
            void CalculateMoveSynergy(Func<MoveData, bool> currMovePred, Predicate<MoveData> moveChoicePred, float intensity)
            {
                int count = currentMovesProcessed.Count(currMovePred);
                if (count > 0)
                {
                    metrics.Add(m => (moveChoicePred(dataT.GetMoveData(m)) ? intensity : 1) * count);
                }
            }

            // Nightmare or Dream Eater + Sleep move Synergy
            CalculateMoveSynergy(m => m.effect is MoveEffect.DreamEater or MoveEffect.StatusNightmare, m => m.IsSleepStatusMove, needSynergy);
            // Snore or Sleep Talk + Rest Synergy
            CalculateMoveSynergy(m => m.effect is MoveEffect.SleepTalk or MoveEffect.DamageFailUnlessAsleepFlinchChance, m => m.effect == MoveEffect.Rest, needSynergy);
            // Rollout or Ice Ball + Defense Curl Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.MultiTurnBuildup, m => m.effect == MoveEffect.DefPlus1AndPrepForRoll, preferSynergy);
            // Spit Up or Swallow + Stockpile Synergy
            CalculateMoveSynergy(m => m.effect is MoveEffect.SpitUp or MoveEffect.Swallow, m => m.effect == MoveEffect.Stockpile, needSynergy);
            // Stockpile + Spit Up or Swallow Synergy
            CalculateMoveSynergy(m => m.effect == MoveEffect.Stockpile, m => m.effect is MoveEffect.SpitUp or MoveEffect.Swallow, preferSynergy);
            // Sun Move + Sun
            CalculateMoveSynergy(m => m.effect is MoveEffect.Solarbeam or MoveEffect.RecoverHpWeather1 or MoveEffect.RecoverHpWeather2 or MoveEffect.RecoverHpWeather3, m => m.effect == MoveEffect.WeatherSun, preferSynergy);
            // Attacking Fire Move + Sun
            CalculateMoveSynergy(m => IsAttackMoveOfType(m, PokemonType.FIR), m => m.effect == MoveEffect.WeatherSun, weakSynergy);
            // Rain move + Rain
            CalculateMoveSynergy(m => m.effect == MoveEffect.Thunder, m => m.effect == MoveEffect.WeatherRain, preferSynergy);
            // Attacking Water Move + Rain
            CalculateMoveSynergy(m => IsAttackMoveOfType(m, PokemonType.WAT), m => m.effect == MoveEffect.WeatherRain, weakSynergy);
            // Weather Ball + Weather (Rain / Sun / Hail)
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall, m => m.effect is MoveEffect.WeatherRain or MoveEffect.WeatherSun or MoveEffect.WeatherHail, needSynergy);
            // Weather Ball + Sandstorm
            CalculateMoveSynergy(m => m.effect == MoveEffect.WeatherBall, m => m.effect == MoveEffect.WeatherSandstorm, weakSynergy);
            // Charge + Attacking Electric Move
            CalculateMoveSynergy(m => m.effect == MoveEffect.Charge, m => IsAttackMoveOfType(m, PokemonType.ELE), weakSynergy);
            // Endure + Endeavor
            CalculateMoveSynergy(m => m.effect == MoveEffect.Endure, m => m.effect == MoveEffect.Endeavor, weakSynergy);
            // Endure + Flailing Move
            CalculateMoveSynergy(m => m.effect == MoveEffect.Endure, m => m.effect == MoveEffect.DamageMoreAtLowHP, weakSynergy);
            // Flailing Move + Endure
            CalculateMoveSynergy(m => m.effect == MoveEffect.DamageMoreAtLowHP, m => m.effect == MoveEffect.Endure, weakSynergy);
            // Perish song + Trapping move
            CalculateMoveSynergy(m => m.effect is MoveEffect.PerishSong, m => m.IsTrappingMove, preferSynergy);
            // Lock-on + OHKO or low acc move
            CalculateMoveSynergy(m => m.effect is MoveEffect.NextMoveAlwaysHits, m => m.IsVeryLowAccuracy, preferSynergy);
            // OHKO or low acc move + Lock-on
            CalculateMoveSynergy(m => m.IsVeryLowAccuracy, m => m.effect is MoveEffect.NextMoveAlwaysHits, preferSynergy);
            return metrics;
        }
        public Move[] LowAttackMoveSet(PokemonBaseStats pokemon, int level)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var availableMovesKeys = availableMoves.Keys;

            // Choose first move - attempt to choose a damaging status move or DOT move. Favor same type
            var preferredMoves = new WeightedSet<Move>(availableMoves.Count);
            preferredMoves.AddRange(availableMovesKeys, m => LowAttackMoveWeight(pokemon, dataT.GetMoveData(m), availableMoves.Count));
            if (ChooseMoveForIndex(ret, 0, preferredMoves, ref availableMoves))
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
            if (ChooseMoveForIndex(ret, 1, preferredMoves, ref availableMoves))
            {
                return ret;
            }

            
            // Choose third move: level-scaled status
            preferredMoves.Clear();
            preferredMoves.AddRange(GetStatusMoves(availableMoves));
            preferredMoves.Multiply(m => LevelWeightScaleSmall(availableMoves[m]));
            if (ChooseMoveForIndex(ret, 2, preferredMoves, ref availableMoves))
            {
                return ret;
            }

            // Fourth move
            preferredMoves.Clear();
            preferredMoves.AddRange(availableMovesKeys);
            preferredMoves.Multiply(m => LevelWeightScale(availableMoves[m]));
            var metrics = CalculateMoveSynergyMetrics(ret);
            preferredMoves.Multiply(m => MoveSynergyFactor(metrics, m));
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
            if (ChooseMoveForIndex(ret, 3, preferredMoves, ref availableMoves))
            {
                return ret;
            }
            return ret;
        }

        public Move[] CounterAttackMoveSet(PokemonBaseStats pokemon, int level)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var preferredMoves = new WeightedSet<Move>(availableMoves.Count);
            preferredMoves.AddRange(availableMoves.Keys.Where(m => dataT.GetMoveData(m).IsCounterAttack || dataT.GetMoveData(m).effect is MoveEffect.DestinyBond));
            preferredMoves.Multiply(m => LevelWeightScaleSmall(availableMoves[m]));
            // Todo: prefer STAB
            if (ChooseMoveForIndex(ret, 0, preferredMoves, ref availableMoves))
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[0]);
            if (ChooseMoveForIndex(ret, 1, preferredMoves, ref availableMoves))
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[1]);
            if (ChooseMoveForIndex(ret, 2, preferredMoves, ref availableMoves))
            {
                return ret;
            }
            preferredMoves.RemoveIfContains(ret[2]);

            ChooseMoveForIndex(ret, 3, preferredMoves, ref availableMoves);
            return ret;
        }

        public Move[] RandomMoveSet(PokemonBaseStats pokemon, int level)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            var preferredMoves = new WeightedSet<Move>(availableMoves.Keys);
            for(int i = 0; i < 3; ++i)
            {
                if (ChooseMoveForIndex(ret, i, preferredMoves, ref availableMoves))
                {
                    return ret;
                }
                preferredMoves.RemoveIfContains(ret[i]);
            }
            var metrics = CalculateMoveSynergyMetrics(ret);
            preferredMoves.Multiply(m => MoveSynergyFactor(metrics, m));
            // Choose final move
            ChooseMoveForIndex(ret, 3, preferredMoves, ref availableMoves);
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
            else if (moveData.EffectivePower > 0) // Normal Attacking Moves
            { 
                weight = (moveData.EffectivePower * lowAttackingStatPowerModifier) / Math.Max(choiceCount * 0.1f, 1f);
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

        private Dictionary<Move, int> AvailableMoves(PokemonBaseStats pokemon, int level)
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
                return moves;
            }
            var availableMoves = new Dictionary<Move, int>(pokemon.learnSet.Count * 4);
            AddMovesFromPokemon(ref availableMoves, pokemon, level);
            // Add movesets from evolved from (moves down the chain)
            for (var preEvolution = pokemon; preEvolution.evolvesFrom.Count > 0;)
            {
                preEvolution = dataT.GetBaseStats(preEvolution.evolvesFrom[0].Pokemon);
                AddMovesFromPreEvolution(ref availableMoves, pokemon, level, preEvolution);
            }
            return availableMoves;
        }

        private void AddMovesFromPokemon(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level)
        {
            AddMoves(ref availableMoves, pokemon.learnSet.Where(e => e.learnLvl <= level));
        }

        private void AddMovesFromPreEvolution(ref Dictionary<Move, int> availableMoves, PokemonBaseStats pokemon, int level, PokemonBaseStats preEvolution)
        {
            var eligibleMoves = new List<LearnSet.Entry>(preEvolution.learnSet.Count);
            var movesetLookup = pokemon.learnSet.GetMovesLookup();
            foreach(var entry in preEvolution.learnSet)
            {
                // Ignore moves past the learn level and SPLASH
                if(entry.learnLvl > level || entry.move == Move.SPLASH)
                {
                    continue;
                }
                if (availableMoves.ContainsKey(entry.move))
                {
                    if(entry.learnLvl > availableMoves[entry.move])
                    {
                        // Move is higher level, update learn level data
                        availableMoves[entry.move] = entry.learnLvl;
                    }
                    continue;
                }
                if (!movesetLookup.Contains(entry.move))
                {
                    // Pre-evo exclusive move, add to eligible moves
                    eligibleMoves.Add(entry);
                }
            }
            if (eligibleMoves.Count <= 0)
                return;
            AddMoves(ref availableMoves, eligibleMoves);
        }

        private void AddMoves(ref Dictionary<Move, int> availableMoves, IEnumerable<LearnSet.Entry> moves)
        {
            foreach (var entry in moves)
            {
                var move = entry.move;
                var learnLevel = entry.learnLvl;
                if (!availableMoves.ContainsKey(move))
                {
                    availableMoves.Add(move, learnLevel);
                }
                else if (availableMoves[move] < learnLevel)
                {
                    availableMoves[move] = learnLevel;
                }
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
                default:
                    return m => 1;
            };
        }
    }
}
