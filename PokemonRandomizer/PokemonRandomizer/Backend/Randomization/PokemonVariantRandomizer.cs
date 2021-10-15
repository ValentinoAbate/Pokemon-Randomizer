using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;

namespace PokemonRandomizer.Backend.Randomization
{
    public class PokemonVariantRandomizer
    {
        public enum TypeTransformation
        {
            None,
            SecondaryTypeReplacement,
            GainSecondaryType = SecondaryTypeReplacement,
            PrimaryTypeReplacement,
            SingleTypeReplacement = PrimaryTypeReplacement,
            DoubleTypeReplacement,
            TypeLoss,
            GainPrimaryType
        }
        public enum TypeProfile
        {
            Balanced,
            Special,
            Physical
        }

        // Pokemon that require / deserve special variant logic
        private static readonly HashSet<Pokemon> specialPokemon = new HashSet<Pokemon>()
        {
            Pokemon.SMEARGLE,
            Pokemon.SHUCKLE,
            Pokemon.DITTO,
            Pokemon.UNOWN,
            Pokemon.WYNAUT,
            Pokemon.WOBBUFFET,
            Pokemon.CASTFORM,
            Pokemon.DELIBIRD
        };

        private readonly Random rand;
        private readonly IDataTranslator dataT;
        private readonly BonusMoveGenerator bonusMoveGenerator;
        private readonly List<PokemonType> types;
        public PokemonVariantRandomizer(Random rand, IDataTranslator dataT, BonusMoveGenerator bonusMoveGenerator)
        {
            this.rand = rand;
            this.dataT = dataT;
            this.bonusMoveGenerator = bonusMoveGenerator;
            types = new List<PokemonType>(EnumUtils.GetValues<PokemonType>());
            types.Remove(PokemonType.FAI);
            types.Remove(PokemonType.Unknown);
        }

        public void CreateVariant(PokemonBaseStats pokemon, Settings settings)
        {
            if (pokemon.IsVariant)
                return;
            pokemon.IsVariant = true;
            // Variant Data
            var variantData = new VariantData(pokemon)
            {
                // Change to variant type
                TransformationType = ChooseType(pokemon, settings),
            };
            ModifyBaseStats(pokemon, settings, variantData);

            // Create new signature move (if applicable)

            // Modify Learnset
            ModifyLearnset(pokemon, settings, variantData);

            // Modify Color Palette

            // Propagate variance to evolutions
            PropagateVariance(pokemon, settings, variantData);

        }

        private void PropagateVariance(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            var newTypes = new List<PokemonType>(types);
            newTypes.Remove(pokemon.PrimaryType);
            if (!pokemon.IsSingleTyped)
            {
                newTypes.Remove(pokemon.SecondaryType);
            }
            foreach (var evo in pokemon.evolvesTo)
            {
                if (!evo.IsRealEvolution)
                    continue;
                var evolvedPokemon = dataT.GetBaseStats(evo.Pokemon);
                evolvedPokemon.IsVariant = true;
                // New Variant Data
                var evoVariantData = new VariantData(evolvedPokemon)
                {
                    TransformationType = PropagateType(pokemon, evolvedPokemon, newTypes, settings, data),
                    BonusStats = data.BonusStats,
                };
                evoVariantData.MoveReplacements.AddRange(data.MoveReplacements);
                evoVariantData.BonusMoves.AddRange(data.BonusMoves);

                ModifyBaseStats(evolvedPokemon, settings, evoVariantData);

                ModifyLearnset(evolvedPokemon, settings, evoVariantData);

                // Modify Evolution (if applicable)

                // Keep propogating
                PropagateVariance(evolvedPokemon, settings, evoVariantData);
            }
        }

        #region Type Choice

        private TypeTransformation ChooseType(PokemonBaseStats pokemon, Settings settings)
        {
            TypeTransformation transformationType;
            if (pokemon.IsSingleTyped)
            {
                transformationType = rand.Choice(settings.SingleTypeTransformationWeights);
                if (transformationType == TypeTransformation.SingleTypeReplacement)
                {
                    // Change the pokemon into a different single type
                    pokemon.SetSingleType(RandomPrimaryType(types, pokemon.PrimaryType));
                }
                else if(transformationType == TypeTransformation.GainSecondaryType)
                {
                    // Change the pokemon into a dual-typed pokemon by adding a new type
                    pokemon.SecondaryType = RandomSecondaryType(types, pokemon.PrimaryType);
                }
                else if(transformationType == TypeTransformation.DoubleTypeReplacement)
                {
                    // Change the pokemon into a dual-type pokemon with unrelated types
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.PrimaryType);
                    pokemon.SecondaryType = RandomSecondaryType(types, pokemon.types);
                }
            }
            else // Dual Type Pokemon
            {
                transformationType = rand.Choice(settings.DualTypeTransformationWeights);
                // If appropriate, invert primary / secondary chance
                if (settings.InvertChanceOfSecondaryTypeChangingForFlyingTypes && pokemon.IsType(PokemonType.FLY))
                {
                    if (transformationType == TypeTransformation.PrimaryTypeReplacement)
                    {
                        transformationType = TypeTransformation.SecondaryTypeReplacement;
                    }
                    else if (transformationType == TypeTransformation.SecondaryTypeReplacement)
                    {
                        transformationType = TypeTransformation.PrimaryTypeReplacement;
                    }
                }
                if(transformationType == TypeTransformation.PrimaryTypeReplacement)
                {
                    // Change the primary type
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.types);
                }
                else if(transformationType == TypeTransformation.SecondaryTypeReplacement)
                {
                    // Change secondary type
                    pokemon.SecondaryType = RandomSecondaryType(types, pokemon.types);
                }
                else if(transformationType == TypeTransformation.DoubleTypeReplacement)
                {
                    var originalPrimaryType = pokemon.PrimaryType;
                    // Change the pokemon into a dual-type pokemon with unrelated types
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.types);
                    pokemon.SecondaryType = RandomSecondaryType(types, originalPrimaryType, pokemon.PrimaryType, pokemon.SecondaryType);
                }
                else // Type loss
                {
                    pokemon.SetSingleType(RandomPrimaryType(types, pokemon.types));
                }
            }
            if (settings.SafeWonderGuard)
            {
                WonderGuardFix(pokemon);
            }
            return transformationType;
        }

        private TypeTransformation PropagateType(PokemonBaseStats pokemon, PokemonBaseStats evolvedPokemon, List<PokemonType> newTypes, Settings settings, VariantData data)
        {
            TypeTransformation newTransformationType = data.TransformationType;
            // If the evolved pokemon is the same type as the base pokemon originally was, just pass the type changes through
            if (evolvedPokemon.PrimaryType == pokemon.OriginalPrimaryType && evolvedPokemon.SecondaryType == pokemon.OriginalSecondaryType)
            {
                evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                evolvedPokemon.SecondaryType = pokemon.SecondaryType;
            }
            else if (pokemon.OriginallySingleType) // Pattern is either single replacement, gain type, or double replacement
            {
                // base pokemon was single typed, and evolved pokemon is a different single type (single replacement)
                // E.g azurill -> marill
                if (evolvedPokemon.IsSingleTyped)
                {
                    // Pokemon is still single typed: choose a different primary type that is not the base pokemon's new type or the evolved pokemon's old type
                    if (data.TransformationType == TypeTransformation.SingleTypeReplacement)
                    {
                        evolvedPokemon.SetSingleType(RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType));
                    }
                    else if (data.TransformationType == TypeTransformation.GainSecondaryType)
                    {
                        // Base pokemon gained a dual type, propagate the dual type to the evolved pokemon
                        evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        if (evolvedPokemon.IsSingleTyped)
                        {
                            newTransformationType = TypeTransformation.None;
                        }
                    }
                    else if (data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {
                        // Replace the first type with something that isn't the original single type or either of the new types
                        evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                        // Carry over the newly gained secondary type
                        evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        if (evolvedPokemon.IsType(evolvedPokemon.OriginalPrimaryType))
                        {
                            newTransformationType = TypeTransformation.GainPrimaryType;
                        }
                    }
                }
                else if (evolvedPokemon.IsType(pokemon.OriginalPrimaryType)) // pokemon gains a type when evolving (e.g shroomish -> breloom)
                {
                    if (data.TransformationType == TypeTransformation.SingleTypeReplacement)
                    {
                        // Carry over the new primary type
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        newTransformationType = TypeTransformation.PrimaryTypeReplacement;
                    }
                    else if (data.TransformationType == TypeTransformation.GainSecondaryType)
                    {
                        // Keep the secondary type from the evolution and the new secondary type from the base pokemon
                        if (pokemon.SecondaryType == evolvedPokemon.SecondaryType)
                        {
                            // Evolved pokemon is the same type as the base pokemon. Do nothing (maybe add something later)
                        }
                        else if (pokemon.SecondaryType == PokemonType.FLY)
                        {
                            // If the secondary type was flying, reverse the type order
                            evolvedPokemon.PrimaryType = evolvedPokemon.SecondaryType;
                            evolvedPokemon.SecondaryType = PokemonType.FLY;
                            newTransformationType = TypeTransformation.SecondaryTypeReplacement;
                        }
                        else
                        {
                            evolvedPokemon.PrimaryType = pokemon.SecondaryType;
                            newTransformationType = TypeTransformation.PrimaryTypeReplacement;
                        }
                    }
                    else if (data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.types);
                    }
                }
                else // Evolved pokemon turns into two unrelated types (double replacement) (e.g cubone -> alolan marowak)
                {
                    // Always do a double type replacement
                    evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.types);
                    evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.types);
                    newTransformationType = TypeTransformation.DoubleTypeReplacement;
                }
            }
            else // Originally dual typed: Pattern is either single replacement, dual replacement, or type loss
            {
                if (evolvedPokemon.IsSingleTyped) // Type loss (e.g gloom -> bellossom)
                {
                    newTransformationType = TypeTransformation.SingleTypeReplacement;
                    if (data.TransformationType == TypeTransformation.PrimaryTypeReplacement || data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {
                        evolvedPokemon.SetSingleType(pokemon.PrimaryType);
                    }
                    else if (data.TransformationType == TypeTransformation.SecondaryTypeReplacement)
                    {
                        evolvedPokemon.SetSingleType(pokemon.SecondaryType);
                    }
                    else // Type Loss
                    {
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                        newTransformationType = TypeTransformation.GainSecondaryType;
                    }
                }
                else if (evolvedPokemon.IsType(pokemon.OriginalPrimaryType))
                {
                    // Secondary type replacement
                    // E.g nincada -> ninjask, nincada -> shedinja
                    if (data.TransformationType == TypeTransformation.PrimaryTypeReplacement)
                    {
                        // Carry over new primary type
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                    }
                    else if (data.TransformationType == TypeTransformation.SecondaryTypeReplacement)
                    {
                        // Generate new secondary type
                        evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                    }
                    else if (data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {
                        // Carry over new primary type and generate new secondary type
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                        newTransformationType = TypeTransformation.SecondaryTypeReplacement;
                    }
                    else // type loss
                    {
                        evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        if (!evolvedPokemon.IsSingleTyped)
                            newTransformationType = TypeTransformation.PrimaryTypeReplacement;
                    }
                }
                else if (evolvedPokemon.IsType(pokemon.OriginalSecondaryType))
                {
                    // Primary type replacement
                    // E.g swablu -> altaria
                    if (data.TransformationType == TypeTransformation.PrimaryTypeReplacement)
                    {
                        // Generate new primary type
                        evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                    }
                    else if (data.TransformationType == TypeTransformation.SecondaryTypeReplacement)
                    {
                        // Carry over secondary type
                        evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                    }
                    else if (data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {

                        // Carry over new secondary type and generate new primary type
                        evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                        evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        newTransformationType = TypeTransformation.PrimaryTypeReplacement;
                    }
                    else // type loss
                    {
                        if (pokemon.PrimaryType == PokemonType.NRM)
                        {
                            // If the secondary type was flying, reverse the type order
                            evolvedPokemon.SecondaryType = evolvedPokemon.PrimaryType;
                            evolvedPokemon.PrimaryType = PokemonType.NRM;
                            newTransformationType = TypeTransformation.PrimaryTypeReplacement;
                        }
                        else
                        {
                            evolvedPokemon.SecondaryType = pokemon.PrimaryType;
                            newTransformationType = TypeTransformation.SecondaryTypeReplacement;
                        }
                        if (evolvedPokemon.IsSingleTyped)
                            newTransformationType = TypeTransformation.TypeLoss;
                    }
                }
                else // Evolved pokemon turns into two unrelated types (double replacement) (unsure if this ever happens)
                {
                    // Always do a double type replacement
                    evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.types);
                    evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.types);
                    newTransformationType = TypeTransformation.DoubleTypeReplacement;
                }
            }
            // Apply wonder guard fix if necessary and return
            if (settings.SafeWonderGuard)
            {
                WonderGuardFix(pokemon);
            }
            return newTransformationType;
        }

        private void WonderGuardFix(PokemonBaseStats pokemon)
        {
            if (!pokemon.abilities.Contains(Ability.Wonder_Guard))
                return;
            if (pokemon.IsType(PokemonType.DRK) && pokemon.IsType(PokemonType.GHO))
            {
                Logger.main.Info($"{pokemon.Name} is DRK/GHO and has Wonder Guard. Correcting type to GHO or DRK");
                pokemon.SetSingleType(rand.RandomBool() ? PokemonType.DRK : PokemonType.GHO);
            }
        }

        #region Type Choice Helper Methods

        private static readonly PokemonType[] excludeFromPrimary = new PokemonType[] { PokemonType.FLY };
        private static readonly PokemonType[] excludeFromSecondary = new PokemonType[] { PokemonType.NRM };

        private PokemonType RandomPrimaryTypeAndRemove(IList<PokemonType> choices, params PokemonType[] exclude)
        {
            var choice = RandomType(choices.Except(excludeFromPrimary), exclude);
            choices.Remove(choice);
            return choice;
        }
        private PokemonType RandomSecondaryTypeAndRemove(IList<PokemonType> choices, params PokemonType[] exclude)
        {
            var choice = RandomType(choices.Except(excludeFromSecondary), exclude);
            choices.Remove(choice);
            return choice;
        }
        private PokemonType RandomPrimaryType(IEnumerable<PokemonType> choices, params PokemonType[] exclude)
        {
            return RandomType(choices.Except(excludeFromPrimary), exclude);
        }
        private PokemonType RandomSecondaryType(IEnumerable<PokemonType> choices, params PokemonType[] exclude)
        {
            return RandomType(choices.Except(excludeFromSecondary), exclude);
        }
        private PokemonType RandomType(IEnumerable<PokemonType> choices, params PokemonType[] exclude)
        {
            return rand.Choice(choices.Except(exclude));
        }

        #endregion

        #endregion

        #region Base Stats and EVs

        // May also modify EVs
        private void ModifyBaseStats(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            int originalBST = pokemon.BST;
            var oldTypeProfile = ComputeTypeProfile(pokemon.OriginalPrimaryType, pokemon.OriginalSecondaryType);
            var newTypeProfile = ComputeTypeProfile(pokemon.PrimaryType, pokemon.SecondaryType);
            int attackStatTotal = pokemon.Attack + pokemon.SpAttack;

            // Adjust attacking stats
            if(settings.AdjustAttackStats && (attackStatTotal / (double)originalBST) >= 0.25)
            {
                // Compensate for variant types
                int minimumDifference = (int)Math.Round(attackStatTotal * 0.025);
                bool madeStatAdjustment = false;
                foreach (var type in data.VariantTypes)
                {
                    if (IsSpecial(type))
                    {
                        if(pokemon.Attack > (pokemon.SpAttack + minimumDifference))
                        {
                            pokemon.SpAttack = pokemon.Attack;
                            madeStatAdjustment = true;
                            break;
                        }
                    }
                    else if(pokemon.SpAttack > (pokemon.Attack + minimumDifference))
                    {
                        pokemon.Attack = pokemon.SpAttack;
                        madeStatAdjustment = true;
                        break;
                    }
                }
                if (madeStatAdjustment)
                {
                    // Readjust stats to maintain BST for single-type attackers
                    int bstDiff = pokemon.BST - originalBST;
                    if (bstDiff > 0)
                    {
                        if (newTypeProfile == TypeProfile.Special)
                        {
                            pokemon.Attack = DecreaseStat(pokemon.Attack, bstDiff);
                        }
                        else if (newTypeProfile == TypeProfile.Physical)
                        {
                            pokemon.SpAttack = DecreaseStat(pokemon.SpAttack, bstDiff);
                        }
                        else if(settings.ReduceBalancedPokemonBST)
                        {
                            var statChoiceWeight = new WeightedSet<int>(pokemon.stats.Length);
                            while (bstDiff >= 15)
                            {
                                statChoiceWeight.Clear();
                                statChoiceWeight.AddRange(Enumerable.Range(0, pokemon.stats.Length), i => pokemon.stats[i]);
                                int statChoice = rand.Choice(statChoiceWeight);
                                pokemon.stats[statChoice] = DecreaseStat(pokemon.stats[statChoice], 5);
                                bstDiff -= 5;
                            }
                        }
                    }
                    // Adjust EVs if necessary to match adjustment
                    int totalAttackEVs = pokemon.AttackEvYield + pokemon.SpAttackEvYield;
                    if (totalAttackEVs > 0)
                    {
                        if (newTypeProfile == TypeProfile.Physical)
                        {
                            pokemon.AttackEvYield = totalAttackEVs;
                            pokemon.SpAttackEvYield = 0;
                        }
                        else if (newTypeProfile == TypeProfile.Special)
                        {
                            pokemon.AttackEvYield = 0;
                            pokemon.SpAttackEvYield = totalAttackEVs;
                        }
                        else
                        {
                            int avgAttackEVs = totalAttackEVs / 2;
                            pokemon.AttackEvYield = avgAttackEVs;
                            pokemon.SpAttackEvYield = avgAttackEVs;
                            int remainder = totalAttackEVs % 2;
                            if (remainder >= 1)
                            {
                                if (rand.RandomBool())
                                {
                                    pokemon.AttackEvYield++;
                                }
                                else
                                {
                                    pokemon.SpAttackEvYield++;
                                }
                            }
                        }
                    }
                }
            }
            if (settings.GiveBonusStats)
            {
                if(data.BonusStats == null) // Determine bonus stats
                {
                    data.BonusStats = new int[6];
                    // Augment variant pokemon
                    short statGrowth = (short)(rand.RandomGaussianInt(settings.StatChangeMean, settings.StatChangeStdDev) - (pokemon.BST - originalBST));
                    if (statGrowth <= 0)
                        return;
                    const short increment = 5;
                    var statChoiceWeight = new WeightedSet<int>(pokemon.stats.Length);
                    while (statGrowth > 0)
                    {
                        short numstats = statGrowth < increment ? statGrowth : increment;
                        statChoiceWeight.Clear();
                        statChoiceWeight.AddRange(Enumerable.Range(0, pokemon.stats.Length), i => pokemon.stats[i]);
                        int statChoice = rand.Choice(statChoiceWeight);
                        data.BonusStats[statChoice] += numstats;
                        pokemon.stats[statChoice] = IncreaseStat(pokemon.stats[statChoice], numstats);
                        statGrowth -= increment;
                    }
                }
                else // Bonus stats have already been determined, propagate
                {
                    for(int i = 0; i < data.BonusStats.Length; ++i)
                    {
                        if (data.BonusStats[i] <= 0)
                            continue;
                        pokemon.stats[i] = IncreaseStat(pokemon.stats[i], data.BonusStats[i]);
                    }
                }
            }
        }

        private byte IncreaseStat(byte currentValue, int increaseAmount)
        {
            return (byte)Math.Min(currentValue + increaseAmount, 0xFF);
        }

        private byte DecreaseStat(byte currentValue, int decreaseAmount)
        {
            return (byte)Math.Max(currentValue - decreaseAmount, 0x00);
        }

        #endregion

        #region Learnset

        // Any move that shouldn't be replaced, even if it meets all other criteria
        private static readonly HashSet<Move> dontReplace = new HashSet<Move>()
        {
            Move.PAY_DAY,
            Move.FAKE_OUT,
            Move.PAIN_SPLIT,
            Move.SKETCH,
            Move.SLACK_OFF,
            Move.MILK_DRINK,
            Move.SOFTBOILED,
            Move.TRANSFORM,
            Move.METRONOME,
            Move.SUPER_FANG,
            Move.SPIT_UP
        };

        // Pokemon that shouldn't recieve bonus moves, only replacement moves
        private static readonly HashSet<Pokemon> specialLearnsetPokemon = new HashSet<Pokemon>()
        {
            Pokemon.CATERPIE,
            Pokemon.WURMPLE,
            Pokemon.WEEDLE,
            Pokemon.KAKUNA,
            Pokemon.METAPOD,
            Pokemon.SILCOON,
            Pokemon.CASCOON,
            Pokemon.BELDUM,
            Pokemon.MAGIKARP,
            Pokemon.FEEBAS,
            Pokemon.TYROGUE,
            Pokemon.ABRA
        };

        private bool IsAttackingMoveOfType(LearnSet.Entry entry, PokemonType type, int minPower = 0, int maxPower = 0xFF)
        {
            var moveData = dataT.GetMoveData(entry.move);
            if (moveData.IsStatus || moveData.IsCounterAttack)
                return false;
            int effectivePower = moveData.EffectivePower;
            return IsType(moveData, type) &&  effectivePower >= minPower && effectivePower <= maxPower;
        }

        public bool HasAttackingMoveOfType(LearnSet learnSet, PokemonType type, int minPower = 0, int maxPower = 0xFF)
        {
            return learnSet.Any(entry => IsAttackingMoveOfType(entry, type, minPower, maxPower));
        }

        private List<MoveData> GetAvailibleAddMoves(LearnSet learnSet)
        {
            return EnumUtils.GetValues<Move>().Where(m => m != Move.None && !learnSet.Learns(m)).Select(m => dataT.GetMoveData(m)).ToList();
        }

        private List<MoveData> GetAvailibleTypeMoves(IEnumerable<MoveData> allMoves, PokemonType type, LearnSet learnSet)
        {
            var moves = allMoves.Where(m => IsType(m, type) && !learnSet.Learns(m.move)).ToList();
            moves.Sort((m1, m2) => m1.EffectivePower.CompareTo(m2.EffectivePower));
            return moves;
        }

        private LearnSet.Entry[] GetEntriesOfType(PokemonType type, LearnSet learnSet)
        {
            return learnSet.Where(entry => dataT.GetMoveData(entry.move).type == type).ToArray();
        }

        private void ModifyLearnsetSpecial(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            // Apply standard replacements
            var availableAddMoves = GetAvailibleAddMoves(pokemon.learnSet);
            foreach (var typeReplacement in data.TypeReplacements)
            {
                var availibleTypeMoves = GetAvailibleTypeMoves(availableAddMoves, typeReplacement.newType, pokemon.learnSet);
                var movesToReplace = GetEntriesOfType(typeReplacement.originalType, pokemon.learnSet);
                foreach (var entry in movesToReplace)
                {
                    entry.move = ReplaceMove(entry.move, data, ref availibleTypeMoves);
                }
            }
            // Replace normal type attacking moves
            var normalAttackingMoveEntries = pokemon.learnSet.Where(entry => IsAttackingMoveOfType(entry, PokemonType.NRM));
            var availibleNormalReplacementMoves = GetAvailibleTypeMoves(availableAddMoves, data.VariantTypes[0], pokemon.learnSet);
            if(data.VariantTypes.Length > 1)
            {
                availibleNormalReplacementMoves.AddRange(GetAvailibleTypeMoves(availableAddMoves, data.VariantTypes[1], pokemon.learnSet));
            }
            foreach(var entry in normalAttackingMoveEntries)
            {
                entry.move = ReplaceMove(entry.move, data, ref availibleNormalReplacementMoves);
            }
        }

        private void ModifyLearnset(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            if (specialLearnsetPokemon.Contains(pokemon.species))
            {
                ModifyLearnsetSpecial(pokemon, settings, data);
                return;
            }
            // Apply carried over data from evolutionary line
            foreach (var moveReplacement in data.MoveReplacements)
            {
                var newMoveData = dataT.GetMoveData(moveReplacement.newMove);
                var oldMoveData = dataT.GetMoveData(moveReplacement.oldMove);
                // If the pokemon is not currently the type of the new move and was the type of the old move, don't make the replacement
                // This move will be reaplaced with a different move later
                if (!IsType(newMoveData, pokemon.types) && IsType(oldMoveData, pokemon.OriginalTypes.ToArray()))
                    continue;
                foreach(var entry in pokemon.learnSet.Where(entry => entry.move == moveReplacement.oldMove))
                {
                    entry.move = moveReplacement.newMove;
                }
            }
            foreach(var bonusMove in data.BonusMoves)
            {
                var bonusMoveData = dataT.GetMoveData(bonusMove.move);
                // If the pokemon is no longer this type, don't apply the bonus move
                if (!IsType(bonusMoveData, pokemon.types))
                    continue;
                pokemon.learnSet.Add(bonusMove);
            }
            var availableAddMoves = GetAvailibleAddMoves(pokemon.learnSet);
            // Apply signiture move replacement
            // Replace Types
            foreach (var typeReplacement in data.TypeReplacements)
            {
                var availibleTypeMoves = GetAvailibleTypeMoves(availableAddMoves, typeReplacement.newType, pokemon.learnSet);
                var movesToReplace = GetEntriesOfType(typeReplacement.originalType, pokemon.learnSet);
                foreach (var entry in movesToReplace)
                {
                    entry.move = ReplaceMove(entry.move, data, ref availibleTypeMoves);
                }
                // Pokemon still doesn't have any attacking moves of the replacement type, add some!
                if (!HasAttackingMoveOfType(pokemon.learnSet, typeReplacement.newType, 2, 50))
                {
                    AddMove(pokemon, 2, 50, 1, 18, data, ref availibleTypeMoves);
                }
                if(!HasAttackingMoveOfType(pokemon.learnSet, typeReplacement.newType, 51))
                {
                    AddMove(pokemon, 51, 120, 22, 99, data, ref availibleTypeMoves);
                }
            }
            foreach (var gainedType in data.GainedTypes)
            {
                if (data.BonusMoves.Any(entry => IsType(dataT.GetMoveData(entry.move), gainedType)))
                    continue;
                // Add moves in at an appropriate level
                var availibleTypeMoves = GetAvailibleTypeMoves(availableAddMoves, gainedType, pokemon.learnSet);
                if (!HasAttackingMoveOfType(pokemon.learnSet, gainedType, 2, 50))
                {
                    AddMove(pokemon, 2, 50, 1, 18, data, ref availibleTypeMoves);
                }
                if (!HasAttackingMoveOfType(pokemon.learnSet, gainedType, 51, 95))
                {
                    AddMove(pokemon, 51, 95, 22, 55, data, ref availibleTypeMoves);
                }
                if (rand.RandomBool() && !HasAttackingMoveOfType(pokemon.learnSet, gainedType, 90, 255))
                {
                    AddMove(pokemon, 80, 255, 45, 99, data, ref availibleTypeMoves);
                }
                else
                {
                    AddMove(pokemon, 0, 0, 1, 99, data, ref availibleTypeMoves);
                }
            }
        }

        private void AddMove(PokemonBaseStats pokemon, int minPower, int maxPower, int minLevel, int maxLevel, VariantData data, ref List<MoveData> availibleMoves)
        {
            var eligibleMoves = availibleMoves.Where(m => m.EffectivePower >= minPower && m.EffectivePower <= maxPower);
            if(eligibleMoves.Count() <= 0)
            {
                Logger.main.Warning($"Variant Generator: Attemping to add a move to {pokemon.Name} but no eligible move found. Move add will be skipped");
                return;
            }
            // Choose the move
            var move = rand.Choice(eligibleMoves).move;
            int learnLevel = Math.Max(Math.Min(bonusMoveGenerator.GenerateLearnLevel(move), maxLevel), minLevel);
            var entry = new LearnSet.Entry(move, learnLevel);
            pokemon.learnSet.Add(entry);
            data.BonusMoves.Add(entry);
            availibleMoves.RemoveAll(m => m.move == move);
        }

        private Move ReplaceMove(Move move, VariantData data, ref List<MoveData> availibleMoves)
        {
            if (dontReplace.Contains(move))
                return move;
            var oldMove = dataT.GetMoveData(move);
            var newMove = Move.None;
            if (oldMove.HasUndefinedRealPower) // Attempt to swap sheer cold w/ fissure, night shade w/ seismic toss, etc if applicable
            {
                IEnumerable<MoveData> eligibleMoves;
                if (oldMove.IsCounterAttack)
                {
                    eligibleMoves = availibleMoves.Where(m => m.IsCounterAttack);
                }
                else
                {
                    eligibleMoves = availibleMoves.Where(m => m.HasUndefinedRealPower && m.effect == oldMove.effect);
                }
                // Actually replace move
                if (eligibleMoves.Count() > 0)
                {
                    newMove = rand.Choice(eligibleMoves).move;
                }
                else if (oldMove.EffectivePower != 1)
                {
                    newMove = ChooseAttackingMove(availibleMoves, oldMove.EffectivePower, 10);
                }
                else if (oldMove.IsOneHitKO)
                {
                    newMove = ChooseAttackingMove(availibleMoves, 100, 20);
                }
                else // replace with status move
                {
                    newMove = ChooseStatusMove(availibleMoves);
                }
            }
            else if (oldMove.IsStatus)
            {
                if (oldMove.type == PokemonType.NRM)
                    return move;
                newMove = ChooseStatusMove(availibleMoves);
            }
            else
            {
                newMove = ChooseAttackingMove(availibleMoves, oldMove.EffectivePower, 10);
            }
            if(newMove == Move.None || newMove == move)
            {
                return move;
            }
            availibleMoves.RemoveAll(m => m.move == newMove);
            data.MoveReplacements.Add((move, newMove));
            return newMove;
        }

        private static readonly HashSet<(Move move, PokemonType type)> moveTypeOverrides = new HashSet<(Move move, PokemonType type)>()
        {
            (Move.SMOKESCREEN, PokemonType.FIR),
            (Move.WHIRLWIND, PokemonType.FLY),
            (Move.HARDEN, PokemonType.RCK),
            (Move.HARDEN, PokemonType.BUG),
            (Move.GROWTH, PokemonType.GRS),
            (Move.MEAN_LOOK, PokemonType.GHO),
            (Move.CURSE, PokemonType.GHO)
        };

        private bool IsType(MoveData move, params PokemonType[] types)
        { 
            if (types.Any(type => moveTypeOverrides.Contains((move.move, type))))
                return true;
            return types.Any(type => move.type == type);
        }

        private Move ChooseAttackingMove(IEnumerable<MoveData> allMoves, int oldMovePower, int powerDiffTolerance)
        {
            var nonStatusMoves = allMoves.Where(m => !m.IsStatus && !m.IsOneHitKO);
            var eligibleMoves = nonStatusMoves.Where(m => m.EffectivePower >= oldMovePower - powerDiffTolerance && m.EffectivePower <= oldMovePower + powerDiffTolerance);
            if (eligibleMoves.Count() > 0)
            {
                return rand.Choice(eligibleMoves).move;
            }
            else
            {
                Move closestMove = Move.None;
                int closestPowerDifference = int.MaxValue;
                foreach (var move in nonStatusMoves)
                {
                    int powerDiff = Math.Abs(move.EffectivePower - oldMovePower);
                    if (powerDiff < closestPowerDifference)
                    {
                        closestMove = move.move;
                        closestPowerDifference = powerDiff;
                    }
                }
                return closestMove;
            }
        }

        private Move ChooseStatusMove(IEnumerable<MoveData> allMoves)
        {
            var eligibleMoves = allMoves.Where(m => m.IsStatus);
            if (eligibleMoves.Count() > 0)
            {
                return rand.Choice(eligibleMoves).move;
            }
            return Move.None;
        }

        #endregion

        private TypeProfile ComputeTypeProfile(PokemonType primary, PokemonType secondary)
        {
            bool primaryTypeSpecial = IsSpecial(primary);
            if (primaryTypeSpecial != IsSpecial(secondary))
                return TypeProfile.Balanced;
            return primaryTypeSpecial ? TypeProfile.Special : TypeProfile.Physical;
        }
        private bool IsSpecial(PokemonType type)
        {
            return type > PokemonType.Unknown;
        }

        private class VariantData
        {
            private readonly PokemonBaseStats pokemon;
            public VariantData(PokemonBaseStats pokemon)
            {
                this.pokemon = pokemon;
            }
            public TypeTransformation TransformationType { get; set; }

            public PokemonType[] VariantTypes
            {
                get
                {
                    return TransformationType switch
                    {
                        TypeTransformation.SecondaryTypeReplacement => new PokemonType[] { pokemon.SecondaryType },
                        TypeTransformation.PrimaryTypeReplacement => new PokemonType[] { pokemon.PrimaryType },
                        TypeTransformation.DoubleTypeReplacement => new PokemonType[] { pokemon.PrimaryType, pokemon.SecondaryType },
                        TypeTransformation.TypeLoss => new PokemonType[] { pokemon.PrimaryType },
                        TypeTransformation.GainPrimaryType => new PokemonType[] { pokemon.PrimaryType },
                        _ => Array.Empty<PokemonType>()
                    };
                }
            }

            public (PokemonType originalType, PokemonType newType)[] TypeReplacements
            {
                get
                {
                    if (pokemon.OriginallySingleType)
                    {
                        return TransformationType switch
                        {
                            TypeTransformation.SingleTypeReplacement => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalPrimaryType, pokemon.PrimaryType) },
                            TypeTransformation.DoubleTypeReplacement => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalPrimaryType, pokemon.PrimaryType) },
                            _ => Array.Empty<(PokemonType originalType, PokemonType newType)>()
                        };
                    }
                    else
                    {
                        return TransformationType switch
                        {
                            TypeTransformation.SecondaryTypeReplacement => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalSecondaryType, pokemon.SecondaryType) },
                            TypeTransformation.PrimaryTypeReplacement => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalPrimaryType, pokemon.PrimaryType) },
                            TypeTransformation.DoubleTypeReplacement => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalPrimaryType, pokemon.PrimaryType), (pokemon.OriginalSecondaryType, pokemon.SecondaryType)},
                            TypeTransformation.TypeLoss => new (PokemonType originalType, PokemonType newType)[] { (pokemon.OriginalPrimaryType, pokemon.PrimaryType), (pokemon.OriginalSecondaryType, pokemon.SecondaryType) },
                            _ => Array.Empty<(PokemonType originalType, PokemonType newType)>()
                        };
                    }
                }
            }

            // Any types the pokemon has gained that do not replace one of it's original types
            public IEnumerable<PokemonType> GainedTypes
            {
                get
                {
                    // If the pokemon wasn't originally single type or is currently single types, no gained types
                    if (!pokemon.OriginallySingleType || pokemon.IsSingleTyped)
                        return Enumerable.Empty<PokemonType>();
                    return new PokemonType[] { TransformationType == TypeTransformation.GainPrimaryType ? pokemon.PrimaryType : pokemon.SecondaryType };
                }
            }

            public int[] BonusStats { get; set; } = null;

            public List<(Move oldMove, Move newMove)> MoveReplacements { get; } = new List<(Move oldMove, Move newMove)>();
            public List<LearnSet.Entry> BonusMoves { get; } = new List<LearnSet.Entry>();
        }

        public class Settings
        {
            public WeightedSet<TypeTransformation> SingleTypeTransformationWeights { get; set; }
            public WeightedSet<TypeTransformation> DualTypeTransformationWeights { get; set; }
            public bool InvertChanceOfSecondaryTypeChangingForFlyingTypes { get; set; }
            public bool SafeWonderGuard { get; set; } = true;
            public bool AdjustAttackStats { get; set; } = true;
            public bool ReduceBalancedPokemonBST { get; set; } = false;
            public bool GiveBonusStats { get; set; } = true;
            public double StatChangeStdDev { get; set; } = 5;
            public double StatChangeMean { get; set; } = 15;
        }
    }
}
