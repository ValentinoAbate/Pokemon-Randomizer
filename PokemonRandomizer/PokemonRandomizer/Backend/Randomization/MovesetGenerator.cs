using PokemonRandomizer.Backend.DataStructures;
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

        public MovesetGenerator(IDataTranslator dataT, Random rand)
        {
            this.dataT = dataT;
            this.rand = rand;
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
            //if(pokemon.species is Pokemon.SHUCKLE or Pokemon.CHANSEY or Pokemon.BLISSEY or Pokemon.HOPPIP or Pokemon.SKIPLOOM or Pokemon.JUMPLUFF
            //    or Pokemon.LILEEP or Pokemon.CRADILY)
            //{
            //    return StatusTankMoveset(pokemon, level);
            //}
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
                foreach(var m2 in ret)
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
            if(ChooseMoveForIndex(ret, 0, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * LevelFactorLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose second move - attempt to choose another attack move
            if (ChooseMoveForIndex(ret, 1, GetAttackMoves(availableMoves), (m) => PowerFactor(m) * StabBonus(m) * RedundantTypeFactor(m) * LevelFactorLog(m), ref availableMoves))
            {
                return ret;
            }

            // Choose third move - Attempt to choose a status move
            if(ChooseMoveForIndex(ret, 2, GetStatusMoves(availableMoves), LevelFactorSmall, ref availableMoves))
            {
                return ret;
            }

            var fourthMoveChoice = new WeightedSet<Move>(availableMoves.Keys, LevelFactor);
            var currentMoves = ret.Where(m => m != Move.None).Select(dataT.GetMoveData);
            var metrics = new List<Func<Move, float>>();


            // Apply synergy metrics

            // Move Synergies
            void CalculateMoveSynergy(Func<MoveData, bool> currMovePred, Predicate<MoveData> moveChoicePred, float intensity)
            {
                int count = currentMoves.Count(currMovePred);
                if (count > 0)
                {
                    metrics.Add(m => (moveChoicePred(dataT.GetMoveData(m)) ? intensity : 1) * count);
                }
            }

            // Nightmare or Dream Eater + Sleep move Synergy
            CalculateMoveSynergy(m => m.effect is MoveEffect.DreamEater or MoveEffect.StatusNightmare, m => m.effect is MoveEffect.StatusSleep or MoveEffect.Yawn, needSynergy);
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
            // Choose fourth move
            ret[3] = rand.Choice(new WeightedSet<Move>(availableMoves.Keys, m => LevelFactor(m) * Math.Max(1, metrics.Sum((metric) => metric(m)))));

            return ret;
        }

        public Move[] StatusTankMoveset(PokemonBaseStats pokemon, int level)
        {
            // Initialize move choices
            var availableMoves = AvailableMoves(pokemon, level);
            // Initialize returns
            var ret = EmptyMoveset();
            if (availableMoves.Count <= 0)
                return ret;

            // Choose first move - attempt to choose a damaging status move or DOT move. Favor same type
            var preferredMoves = new WeightedSet<Move>(availableMoves.Count);
            foreach(var kvp in availableMoves)
            {
                var moveData = dataT.GetMoveData(kvp.Key);
                float weight = 0;
                if(moveData.effect is MoveEffect.StatusBadlyPoison or MoveEffect.StatusLeechSeed)
                {
                    weight = 1000;
                }
                else if(moveData.effect is MoveEffect.StatusBurn or MoveEffect.StatusPoison)
                {
                    weight = 100;
                }
                else if(moveData.effect is MoveEffect.DoTTrap or MoveEffect.StatusConfuse or MoveEffect.StatusConfuseAll or MoveEffect.StatusConfuseAtkPlus2 or MoveEffect.StatusConfuseSpAtkPlus2 or MoveEffect.PerishSong)
                {
                    weight = 10;
                }
                else if(moveData.effect is MoveEffect.StatusNightmare)
                {
                    foreach(var entry in pokemon.learnSet)
                    {
                        var entryData = dataT.GetMoveData(entry.move);
                        if(entryData.effect is MoveEffect.StatusSleep or MoveEffect.Yawn)
                        {
                            weight = 100;
                            break;
                        }
                    }
                }
                else if(moveData.effect is MoveEffect.Curse)
                {
                    if (pokemon.IsType(PokemonType.GHO))
                    {
                        weight = 100;
                    }
                }
                else if(moveData.effect is MoveEffect.WeatherSandstorm)
                {
                    if (pokemon.IsType(PokemonType.RCK))
                    {
                        weight = 20; // Going to be multiplied by 2 for STAB anyway
                    }
                    else if(pokemon.IsType(PokemonType.GRD) || pokemon.IsType(PokemonType.STL))
                    {
                        weight = 10;
                    }
                }
                else if (moveData.effect is MoveEffect.WeatherHail)
                {
                    if (pokemon.IsType(PokemonType.ICE))
                    {
                        weight = 10;
                    }
                }
                if (moveData.IsType(pokemon))
                {
                    weight *= 10;
                }
                if(weight > 0)
                {
                    preferredMoves.Add(moveData.move, weight);
                }
            }
            if (ChooseMoveForIndex(ret, 0, preferredMoves, ref availableMoves))
            {
                return ret;
            }
            // Choose second move - attempt to choose another damaging status move or DOT move that can overlap with the first one
            var firstMoveData = dataT.GetMoveData(ret[0]);
            preferredMoves.RemoveIfContains(firstMoveData.move);
            // Don't stack non-volatile status conditions
            if(firstMoveData.effect is MoveEffect.StatusPoison or MoveEffect.StatusBurn or MoveEffect.StatusBadlyPoison)
            {
                preferredMoves.RemoveWhere(m => dataT.GetMoveData(m).effect is MoveEffect.StatusPoison or MoveEffect.StatusBurn or MoveEffect.StatusBadlyPoison);
            }
            else if(firstMoveData.effect is MoveEffect.StatusNightmare)
            {
                // If nightmare, ensure sleep
                preferredMoves.RemoveWhere(m => dataT.GetMoveData(m).effect is not (MoveEffect.StatusSleep or MoveEffect.Yawn));
            }
            else if (firstMoveData.effect is MoveEffect.WeatherSandstorm or MoveEffect.WeatherHail)
            {
                // Don't stack weather conditions
                preferredMoves.RemoveWhere(m => dataT.GetMoveData(m).effect is MoveEffect.WeatherSandstorm or MoveEffect.WeatherHail);
            }
            if (ChooseMoveForIndex(ret, 1, preferredMoves, ref availableMoves))
            {
                return ret;
            }

            // Choose third move = attempt to choose a buff / disruption move:
            // Priority 1: healing move (recover / synth (etc.)) > ingrain > wish > drain move
            // Priority 2: protect / detect (endure?)
            // Priority 3: def/sp.def/evasion buff
            // Priority 4: attack/sp.attack/acc debuff
            if (ChooseMoveForIndex(ret, 2, new WeightedSet<Move>(GetStatusMoves(availableMoves)), ref availableMoves))
            {
                return ret;
            }

            // Fourth move
            // Priority 1: flat damage (dragon rage, psywave, seismic toss, night shade, sonicboom, dragon rage, etc)
            // Priority 2: OHKO
            // Priority 3: def/sp.def/evasion buff
            // Else Level-based
            if (ChooseMoveForIndex(ret, 3, new WeightedSet<Move>(GetStatusMoves(availableMoves)), ref availableMoves))
            {
                return ret;
            }

            return ret;
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
