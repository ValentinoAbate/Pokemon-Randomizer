﻿using System;
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

            ModifyAbilities(pokemon, settings, variantData);

            // Create new signature move (if applicable)

            // Modify Learnset
            ModifyLearnset(pokemon, settings, variantData);

            // Modify Color Palette
            ModifyPalette(pokemon, settings, variantData);

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
                    AbilityReplacements = new Dictionary<Ability, Ability>(data.AbilityReplacements)
                };
                evoVariantData.MoveReplacements.AddRange(data.MoveReplacements);
                evoVariantData.BonusMoves.AddRange(data.BonusMoves);

                if (evoVariantData.TransformationType == TypeTransformation.None)
                    continue;

                ModifyBaseStats(evolvedPokemon, settings, evoVariantData);

                ModifyAbilities(evolvedPokemon, settings, evoVariantData);

                ModifyLearnset(evolvedPokemon, settings, evoVariantData);

                // Modify Evolution (if applicable)
                ModifyPalette(evolvedPokemon, settings, evoVariantData);

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
                else if (transformationType == TypeTransformation.GainSecondaryType)
                {
                    // Change the pokemon into a dual-typed pokemon by adding a new type
                    pokemon.SecondaryType = RandomSecondaryType(types, pokemon.PrimaryType);
                }
                else if (transformationType == TypeTransformation.DoubleTypeReplacement)
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
                if (transformationType == TypeTransformation.PrimaryTypeReplacement)
                {
                    // Change the primary type
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.types);
                }
                else if (transformationType == TypeTransformation.SecondaryTypeReplacement)
                {
                    // Change secondary type
                    pokemon.SecondaryType = RandomSecondaryType(types, pokemon.types);
                }
                else if (transformationType == TypeTransformation.DoubleTypeReplacement)
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
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                            newTransformationType = TypeTransformation.GainPrimaryType;
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
            if (evolvedPokemon.PrimaryType == evolvedPokemon.OriginalPrimaryType && evolvedPokemon.SecondaryType == evolvedPokemon.OriginalSecondaryType)
            {
                Logger.main.Error($"None transformation type detected for {pokemon.Name} -> {evolvedPokemon.Name}");
                return TypeTransformation.None;
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
            if (settings.AdjustAttackStats && (attackStatTotal / (double)originalBST) >= 0.25)
            {
                // Compensate for variant types
                int minimumDifference = (int)Math.Round(attackStatTotal * 0.025);
                bool madeStatAdjustment = false;
                foreach (var type in data.VariantTypes)
                {
                    if (IsSpecial(type))
                    {
                        if (pokemon.Attack > (pokemon.SpAttack + minimumDifference))
                        {
                            pokemon.SpAttack = pokemon.Attack;
                            madeStatAdjustment = true;
                            break;
                        }
                    }
                    else if (pokemon.SpAttack > (pokemon.Attack + minimumDifference))
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
                        else if (settings.ReduceBalancedPokemonBST)
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
                if (data.BonusStats == null) // Determine bonus stats
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
                    for (int i = 0; i < data.BonusStats.Length; ++i)
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

        #region Abilities

        private readonly Dictionary<Ability, PokemonType> replaceableAbilities = new Dictionary<Ability, PokemonType>()
        {
            { Ability.Torrent, PokemonType.WAT },
            { Ability.Blaze, PokemonType.FIR},
            { Ability.Overgrow, PokemonType.GRS },
            { Ability.Swarm, PokemonType.BUG },
        };

        private readonly Dictionary<PokemonType, Ability[]> replacementAbilities = new Dictionary<PokemonType, Ability[]>
        {
            { PokemonType.WAT, new Ability[] { Ability.Torrent } },
            { PokemonType.FIR, new Ability[] { Ability.Blaze } },
            { PokemonType.GRS, new Ability[] { Ability.Overgrow } },
            { PokemonType.BUG, new Ability[] { Ability.Swarm } },
            { PokemonType.FTG, new Ability[] { Ability.Guts } },
            { PokemonType.ELE, new Ability[] { Ability.Plus, Ability.Minus } },
        };

        private void ModifyAbilities(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            for(int i = 0; i< pokemon.abilities.Length; ++i)
            {

                var ability = pokemon.abilities[i];
                if (data.AbilityReplacements.ContainsKey(ability))
                {
                    pokemon.abilities[i] = ability = data.AbilityReplacements[ability];
                }
                if (ability == Ability.NONE || !replaceableAbilities.ContainsKey(ability))
                    continue;
                var type = replaceableAbilities[ability];
                if (pokemon.IsType(type))
                    continue;
                Ability[] replacements = null;
                if (data.TryGetReplacementType(type, out var newType) && replacementAbilities.ContainsKey(newType))
                {
                    replacements = replacementAbilities[newType];
                }
                else
                {
                    foreach(var varType in pokemon.types)
                    {
                        if (replacementAbilities.ContainsKey(varType))
                            replacements = replacementAbilities[varType];
                    }
                }
                if (replacements == null)
                    continue;
                var newAbility = replacements.Length == 1 ? replacements[0] : rand.Choice(replacements);
                pokemon.abilities[i] = newAbility;
                data.AbilityReplacements.Add(ability, newAbility);
            }
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
            Move.SPIT_UP,
            Move.TAIL_GLOW
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
            return IsType(moveData, type) && effectivePower >= minPower && effectivePower <= maxPower;
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
            if (data.VariantTypes.Length > 1)
            {
                availibleNormalReplacementMoves.AddRange(GetAvailibleTypeMoves(availableAddMoves, data.VariantTypes[1], pokemon.learnSet));
            }
            foreach (var entry in normalAttackingMoveEntries)
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
                foreach (var entry in pokemon.learnSet.Where(entry => entry.move == moveReplacement.oldMove))
                {
                    entry.move = moveReplacement.newMove;
                }
            }
            foreach (var bonusMove in data.BonusMoves)
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
                if (!HasAttackingMoveOfType(pokemon.learnSet, typeReplacement.newType, 51))
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
            if (eligibleMoves.Count() <= 0)
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
            if (newMove == Move.None || newMove == move)
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

        #region Palette

        private static int[] Range(int start, int end)
        {
            return Enumerable.Range(start, 1 + (end - start)).ToArray();
        }
        private static int[] PalRange(params int[] indices)
        {
            return indices;
        }

        private static readonly Dictionary<Pokemon, PaletteData> variantPaletteData = new Dictionary<Pokemon, PaletteData>()
        {
            { Pokemon.BULBASAUR, new PaletteData(new int[]{ 2, 3, 4, 5 }, new int[]{ 11, 12, 13, 14 }) }, // Needs outline check
            { Pokemon.IVYSAUR, new PaletteData(new int[]{ 6, 7, 10, 12 }, new int[]{ 2, 8, 9, }, null,  new int[]{ 3, 4, 5, 13, 14, 15 }) },
            // 1 is a shared outline color
            { Pokemon.VENUSAUR, new PaletteData(new int[]{ 1, 2, 3, 4 }, new int[]{ 10, 13, 14, }, null,  new int[]{ 5, 6, 8, 9 }) },
            //{ Pokemon.CHARMANDER, new PaletteData(new int[]{ 9, 10, 11, 12 }, new int[]{ 4, 5, 6, 7, 8}) },
            { Pokemon.CHARMANDER, new PaletteData(new int[]{ 9, 10, 11, 12 }) },
            //{ Pokemon.CHARMELEON, new PaletteData(new int[]{ 10, 11, 12, 13 }, new int[]{ 4, 5, 6, 7, 8}) },
            { Pokemon.CHARMELEON, new PaletteData(new int[]{ 10, 11, 12, 13 }) },
            { Pokemon.CHARIZARD, new PaletteData(new int[]{ 10, 11, 12, 13 }, new int[]{ 1, 2, 6, 7, 8 }, null, new int[]{ 3, 4, 5 }) },
            { Pokemon.SQUIRTLE, new PaletteData(new int[]{ 11, 12, 13, 14 }, new int[]{ 2, 3, 4 })},
            { Pokemon.WARTORTLE, new PaletteData(new int[]{ 11, 12, 13, 14 }, new int[]{ 5, 6, 7 })},
            { Pokemon.BLASTOISE, new PaletteData(new int[]{ 11, 12, 13, 14 }, new int[]{ 4, 8, 9, 10 })},
            { Pokemon.CATERPIE, new PaletteData(new int[]{ 9, 10, 11, 12 }, new int[]{ 3, 4, 8 }, null, new int[]{ 2, 5, 6, 7 })},
            { Pokemon.METAPOD, new PaletteData(new int[]{ 2, 3, 4, 5 })},
            { Pokemon.BUTTERFREE, new PaletteData(new int[]{ 4, 5, 6, 7 },  new int[]{ 14, 15 })},
            { Pokemon.WEEDLE, new PaletteData(new int[]{ 4, 5, 6, 7, 13 },  new int[]{ 2, 3 })},
            { Pokemon.KAKUNA, new PaletteData(new int[]{ 2, 3, 4, 5, 6 })},
            { Pokemon.BEEDRILL, new PaletteData(Range(2, 5), new int[]{ 10, 11, 12 })},
            { Pokemon.PIDGEY, new PaletteData(Range(6, 12), Range(3, 5))}, // Pidgey line doesn't do well with light primary / dark secondary colors
            { Pokemon.PIDGEOTTO, new PaletteData(Range(6, 11), Range(3, 5))},
            { Pokemon.PIDGEOT, new PaletteData(new int[]{ 6, 7, 8, 11, 12, 13, 14 }, new int[]{ 3, 4, 5, 9, 10 })},
            { Pokemon.RATTATA, new PaletteData(Range(7, 10))},
            { Pokemon.RATICATE, new PaletteData(Range(8, 11), Range(2, 7))},
            { Pokemon.SPEAROW, new PaletteData(Range(10, 13), PalRange(6, 7, 8, 9, 14), Range(2, 5)) },
            { Pokemon.FEAROW, new PaletteData(PalRange(2, 3, 10, 11, 12, 13), PalRange(6, 7, 8, 9, 14)) },
            { Pokemon.EKANS, new PaletteData(Range(12, 15), Range(6, 9))},
            { Pokemon.ARBOK, new PaletteData(Range(9, 12), Range(5, 7))},
            { Pokemon.PIKACHU, new PaletteData(Range(2, 6), Range(10, 12))}, // , Range(10, 12) // Cheeks
            { Pokemon.RAICHU, new PaletteData(Range(2, 4), Range(6, 12))},
            { Pokemon.SANDSHREW, new PaletteData(Range(3, 6))},
            { Pokemon.SANDSLASH, new PaletteData(Range(2, 5), Range(10, 13))},
            { Pokemon.NIDORAN_GAL, new PaletteData(Range(1, 5), Range(8, 10))},
            { Pokemon.NIDORINA, new PaletteData(Range(10, 14), Range(4, 6))},
            { Pokemon.NIDOQUEEN, new PaletteData(Range(10, 14), Range(3, 6))},
            { Pokemon.NIDORAN_BOI, new PaletteData(Range(10, 13), Range(6, 8))},
            { Pokemon.NIDORINO, new PaletteData(Range(10, 13), Range(5, 7))},
            { Pokemon.NIDOKING, new PaletteData(Range(11, 14), Range(8, 10))},
            { Pokemon.CLEFAIRY, new PaletteData(Range(11, 14), Range(6, 9))},
            { Pokemon.CLEFABLE, new PaletteData(Range(5, 8), Range(12, 14))},
            { Pokemon.VULPIX, new PaletteData(Range(12, 15), Range(8, 11))}, // Paws: 5-7
            { Pokemon.NINETALES, new PaletteData(Range(11, 14), Range(2, 4))}, // Eyes: 2-4
            { Pokemon.JIGGLYPUFF, new PaletteData(Range(12, 15), Range(2, 5))},
            { Pokemon.WIGGLYTUFF, new PaletteData(Range(11, 15), Range(2, 5))},
            { Pokemon.ZUBAT, new PaletteData(Range(6, 9), Range(10, 13))},
            { Pokemon.GOLBAT, new PaletteData(Range(5, 8), Range(9, 12))},
            { Pokemon.ODDISH, new PaletteData(Range(4, 7), Range(11, 14))},
            { Pokemon.GLOOM, new PaletteData(Range(7, 10), Range(11, 14), Range(2, 5))}, // Extra leaves: 2-5
            { Pokemon.VILEPLUME, new PaletteData(Range(5, 8), Range(9, 14), Range(2, 5))},
            { Pokemon.PARAS, new PaletteData(Range(10, 13), Range(6, 8))},
            { Pokemon.PARASECT, new PaletteData(Range(11, 14), Range(6, 9))},
            { Pokemon.VENONAT, new PaletteData(Range(11, 14), PalRange(2, 5, 6, 7), null, Range(8, 10))},
            { Pokemon.VENOMOTH, new PaletteData(Range(12, 15), Range(4, 7))},
            { Pokemon.DIGLETT, new PaletteData(Range(11, 14), Range(5, 8))}, // 2-4 nose
            { Pokemon.DUGTRIO, new PaletteData(Range(11, 14), Range(5, 8))}, // 2-4 nose
            { Pokemon.MEOWTH, new PaletteData(Range(12, 15), Range(9, 11), null, Range(5, 6))},
            { Pokemon.PERSIAN, new PaletteData(Range(12, 15), Range(4, 6))},
            { Pokemon.PSYDUCK, new PaletteData(Range(11, 14), Range(1, 4))},
            { Pokemon.GOLDUCK, new PaletteData(Range(11, 14), Range(6, 9))},
            { Pokemon.MANKEY, new PaletteData(Range(12, 15), Range(8, 11))}, // 3-5 nose
            { Pokemon.PRIMEAPE, new PaletteData(Range(12, 15), Range(7, 11))}, // 5-6 nose
            { Pokemon.GROWLITHE, new PaletteData(Range(11, 14), Range(7, 10))},
            { Pokemon.ARCANINE, new PaletteData(Range(11, 14), Range(7, 10))},
            { Pokemon.POLIWAG, new PaletteData(Range(11, 14), Range(8, 10))},
            { Pokemon.POLIWHIRL, new PaletteData(Range(11, 14))},
            { Pokemon.POLIWRATH, new PaletteData(Range(11, 14))},
            { Pokemon.ABRA, new PaletteData(Range(11, 14), Range(5, 8))},
            { Pokemon.KADABRA, new PaletteData(Range(11, 14), Range(5, 8), null, Range(3, 4))},
            { Pokemon.ALAKAZAM, new PaletteData(Range(11, 14), Range(5, 8))},
            { Pokemon.MACHOP, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.MACHOKE, new PaletteData(Range(11, 14), Range(2, 5), null, Range(7, 9))},
            { Pokemon.MACHAMP, new PaletteData(Range(11, 14))}, // Mouth and fins Range(2, 5)
            { Pokemon.BELLSPROUT, new PaletteData(Range(6, 8), Range(9, 11), null, Range(3, 5))},
            { Pokemon.WEEPINBELL, new PaletteData(Range(6, 8), Range(9, 11), null, Range(2, 5))},
            { Pokemon.VICTREEBEL, new PaletteData(PalRange(2, 6, 7, 8), Range(9, 11), null, Range(4, 5))},
            { Pokemon.TENTACOOL, new PaletteData(Range(11, 14), Range(3, 5))},
            { Pokemon.TENTACRUEL, new PaletteData(Range(11, 14), Range(3, 5))},
            { Pokemon.GEODUDE, new PaletteData(Range(11, 14))},
            { Pokemon.GRAVELER, new PaletteData(Range(12, 15))},
            { Pokemon.GOLEM, new PaletteData(Range(5, 8), Range(9, 12))},
            { Pokemon.PONYTA, new PaletteData(Range(2, 5), Range(9, 12))}, // 2-5 is body (should likely be 2ndary. 9-12 is flames (hue gradient not value gradient, needs more advanced processing)
            { Pokemon.RAPIDASH, new PaletteData(Range(2, 6), Range(9, 12))}, // 2-6 is body (should likely be 2ndary. 9-12 is flames (hue gradient not value gradient, needs more advanced processing)
            { Pokemon.SLOWPOKE, new PaletteData(Range(10, 14), Range(3, 6))},
            { Pokemon.SLOWBRO, new PaletteData(Range(11, 14), Range(7, 9), null, Range(2, 5))},
            { Pokemon.MAGNEMITE, new PaletteData(Range(11, 14), Range(3, 4), Range(5, 6))},
            { Pokemon.MAGNETON, new PaletteData(Range(11, 14), Range(3, 4), Range(5, 6))},
            { Pokemon.FARFETCHD, new PaletteData(Range(11, 14), Range(3, 4), null, Range(9, 10))}, // 5-6 break and feet
            { Pokemon.DODUO, new PaletteData(Range(5, 8), Range(9, 11))},
            { Pokemon.DODRIO, new PaletteData(Range(1, 3), Range(4, 7), null, Range(8, 10))},
            { Pokemon.SEEL, new PaletteData(Range(1, 4), Range(5, 7))},
            { Pokemon.DEWGONG, new PaletteData(Range(1, 4))},
            { Pokemon.GRIMER, new PaletteData(Range(5, 8))}, // Mouth 2-4
            { Pokemon.MUK, new PaletteData(Range(1, 5))}, // Mouth 7-9
            { Pokemon.SHELLDER, new PaletteData(Range(1, 5))}, // Tounge 6-8
            { Pokemon.CLOYSTER, new PaletteData(Range(2, 5))},
            { Pokemon.GASTLY, new PaletteData(Range(6, 9), Range(10, 12))}, // 10-12 is gas, this might need to be flipped
            { Pokemon.HAUNTER, new PaletteData(Range(1, 4), Range(9, 11))},
            { Pokemon.GENGAR, new PaletteData(Range(11, 14), Range(4, 6))},
            { Pokemon.ONIX, new PaletteData(Range(10, 14))},
            { Pokemon.DROWZEE, new PaletteData(Range(11, 14), Range(7, 10))},
            { Pokemon.HYPNO, new PaletteData(PalRange(1, 2, 3, 4, 7), Range(12, 13))}, // may also want to include 6 in primary. secondary may be unneccessary
            { Pokemon.KRABBY, new PaletteData(Range(11, 14))},
            { Pokemon.KINGLER, new PaletteData(Range(1, 4))},
            { Pokemon.VOLTORB, new PaletteData(Range(10, 14))},
            { Pokemon.ELECTRODE, new PaletteData(Range(10, 14))},
            { Pokemon.EXEGGCUTE, new PaletteData(Range(11, 15))}, // 3-5 cracked egg
            { Pokemon.EXEGGUTOR, new PaletteData(Range(8, 11), Range(5, 7))}, // 1-4 heads
            { Pokemon.CUBONE, new PaletteData(Range(1, 4), Range(9, 11))},
            { Pokemon.MAROWAK, new PaletteData(Range(12, 15), Range(8, 11))},
            { Pokemon.HITMONLEE, new PaletteData(Range(1, 5), Range(11, 13))},
            { Pokemon.HITMONCHAN, new PaletteData(Range(11, 14), Range(2, 5))}, // 6-9 gloves
            { Pokemon.LICKITUNG, new PaletteData(Range(10, 14), Range(5, 8))},
            { Pokemon.KOFFING, new PaletteData(Range(11, 14), Range(9, 10))}, // 3-4 cross
            { Pokemon.WEEZING, new PaletteData(Range(10, 14), Range(7, 9))}, // 4 cross
            { Pokemon.RHYHORN, new PaletteData(Range(1, 5))},
            { Pokemon.RHYDON, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.CHANSEY, new PaletteData(Range(11, 15), Range(8, 10))},
            { Pokemon.TANGELA, new PaletteData(Range(1, 4), Range(5, 7))},
            { Pokemon.KANGASKHAN, new PaletteData(Range(12, 15), Range(9, 11))}, // 3-5 belly and ears
            { Pokemon.HORSEA, new PaletteData(Range(5, 8), PalRange(4, 9, 10))},
            { Pokemon.SEADRA, new PaletteData(Range(2, 6), Range(7, 9))},
            { Pokemon.GOLDEEN, new PaletteData(Range(6, 11), null, PalRange(5))},
            { Pokemon.SEAKING, new PaletteData(PalRange(5, 6, 9, 10, 11, 12), null, PalRange(13))},
            { Pokemon.STARYU, new PaletteData(Range(6, 9), Range(2, 5), Range(10, 13))},
            { Pokemon.STARMIE, new PaletteData(Range(1, 4), Range(5, 8), Range(10, 13))},
            { Pokemon.MR_MIME, new PaletteData(Range(5, 8), Range(10, 12))},
            { Pokemon.SCYTHER, new PaletteData(Range(11, 14), Range(7, 10), null, Range(5, 6))},
            { Pokemon.JYNX, new PaletteData(Range(10, 13), Range(6, 8))},
            { Pokemon.ELECTABUZZ, new PaletteData(Range(11, 15))},
            { Pokemon.MAGMAR, new PaletteData(Range(11, 14))}, // 7-10 body 2 and part of flame
            { Pokemon.PINSIR, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.TAUROS, new PaletteData(Range(7, 10), Range(11, 14), Range(2, 5))},
            { Pokemon.MAGIKARP, new PaletteData(Range(12, 15), Range(6, 8), null, Range(9, 11))},
            { Pokemon.GYARADOS, new PaletteData(Range(4, 7), Range(8, 10), null, Range(1, 13))},
            { Pokemon.LAPRAS, new PaletteData(Range(11, 14), Range(7, 10))}, // Belly Range(4, 6)
            { Pokemon.DITTO, new PaletteData(Range(5, 9))},
            { Pokemon.EEVEE, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.VAPOREON, new PaletteData(Range(11, 14), Range(7, 9), null, Range(4, 6))},
            { Pokemon.JOLTEON, new PaletteData(Range(1, 5))},
            { Pokemon.FLAREON, new PaletteData(Range(12, 15), Range(2, 5))},
            { Pokemon.PORYGON, new PaletteData(Range(1, 4), Range(5, 8))},
            { Pokemon.OMANYTE, new PaletteData(Range(12, 15), Range(7, 11))},
            { Pokemon.OMASTAR, new PaletteData(Range(12, 15), Range(7, 11))},
            { Pokemon.KABUTO, new PaletteData(Range(11, 14))},
            { Pokemon.KABUTOPS, new PaletteData(Range(11, 14))},
            { Pokemon.AERODACTYL, new PaletteData(Range(11, 14), Range(4, 7))},
            { Pokemon.SNORLAX, new PaletteData(Range(1, 4), Range(5, 8), Range(9, 11))},
            { Pokemon.ARTICUNO, new PaletteData(Range(1, 6), Range(9, 11))}, // maybe move type 1 highlight to type 2
            { Pokemon.ZAPDOS, new PaletteData(Range(11, 15))}, // 8-10 beak and feet
            { Pokemon.MOLTRES, new PaletteData(Range(4, 7), Range(11, 14), Range(8, 10))},
            { Pokemon.DRATINI, new PaletteData(Range(11, 14))},
            { Pokemon.DRAGONAIR, new PaletteData(Range(11, 14), Range(5, 7))},
            { Pokemon.DRAGONITE, new PaletteData(Range(2, 5), Range(6, 8))},
            { Pokemon.MEWTWO, new PaletteData(Range(1, 4), Range(5, 8))},
            { Pokemon.MEW, new PaletteData(Range(10, 14), Range(3, 4))},
            { Pokemon.CHIKORITA, new PaletteData(Range(2, 5), Range(6, 9))},
            { Pokemon.BAYLEEF, new PaletteData(Range(2, 6), Range(7, 10))},
            { Pokemon.MEGANIUM, new PaletteData(Range(2, 5), Range(7, 10), null, Range(14, 15))},
            { Pokemon.CYNDAQUIL, new PaletteData(Range(2, 5), Range(6, 9), Range(10, 13))}, // 10 - 13 is fire
            { Pokemon.QUILAVA, new PaletteData(Range(2, 5), Range(6, 9), Range(10, 13))}, // 10 - 13 is fire
            { Pokemon.TYPHLOSION, new PaletteData(Range(2, 5), Range(6, 9), Range(10, 13))}, // 10 - 13 is fire
            { Pokemon.TOTODILE, new PaletteData(Range(2, 5), Range(8, 10), null, Range(13, 14))}, // 13-14 is stripe
            { Pokemon.CROCONAW, new PaletteData(Range(2, 5), Range(8, 11), null, Range(12, 14))}, // 12-14 is belly
            { Pokemon.FERALIGATR, new PaletteData(Range(2, 5), Range(7, 10), null, Range(11, 14))}, // 11-14 is stripe
        };

        private static readonly Dictionary<PokemonType, TypeColorData> typeColorData = new Dictionary<PokemonType, TypeColorData>()
        {
            {PokemonType.FIR, new TypeColorData(new Color(26, 9, 9, 0)) },
            {PokemonType.GRS, new TypeColorData(new Color(13, 25, 8, 0)) },
            {PokemonType.WAT, new TypeColorData(new Color(16, 20, 28, 0)) },
            {PokemonType.ICE, new TypeColorData(new Color(21, 26, 31, 0)) },
            {PokemonType.FLY, new TypeColorData(new Color(25, 26, 27, 0)) },
            {PokemonType.ELE, new TypeColorData(new Color(31, 31, 15, 0)) { ValueOffset = 1 }},
            {PokemonType.BUG, new TypeColorData(new Color(28, 31, 14, 0)) },
            {PokemonType.GRD, new TypeColorData(new Color(27, 24, 4, 0)) { ValueOffset = -1 }},
            {PokemonType.RCK, new TypeColorData(new Color(12, 13, 4, 0)) { ValueOffset = -3 }},
            {PokemonType.STL, new TypeColorData(new Color(23, 25, 24, 0)){ ValueOffset = -2 }},
            {PokemonType.DRK, new TypeColorData(new Color(11, 10, 10, 0)) { ValueOffset = -8 } },
            {PokemonType.PSN, new TypeColorData(new Color(23, 15, 22, 0)) },
            {PokemonType.GHO, new TypeColorData(new Color(10, 6, 11, 0)) { ValueOffset = -6 }},
            {PokemonType.PSY, new TypeColorData(new Color(22, 14, 28, 0)) }, // 22, 16, 26 
            {PokemonType.FTG, new TypeColorData(new Color(20, 15, 11, 0)) },
            {PokemonType.DRG, new TypeColorData(new Color(10, 20, 26, 0)) },
            {PokemonType.NRM, new TypeColorData(new Color(21, 15, 11, 0)) } // 31, 26, 22
        };

        private static readonly int[] allIndices = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        private void ModifyPalette(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            if (data.TransformationType == TypeTransformation.None)
                return;
            // If we don't have specific palette data or type color data to support this pokemon / type combo, return
            // Perhaps I should add a fallback for pokemon I haven't done specific work for
            if (!variantPaletteData.ContainsKey(pokemon.species))
            {
                ApplyColorChanges(pokemon.palette, allIndices, typeColorData[data.VariantTypes[0]]);
                return;
            }
            ApplyColorsFirstSecond(pokemon, data);
        }

        private void ApplyColorsFirstSecond(PokemonBaseStats pokemon, VariantData data)
        {
            var firstVariantType = data.VariantTypes[0];
            var typeData = typeColorData[firstVariantType];
            var paletteData = variantPaletteData[pokemon.species];
            ApplyColorChanges(pokemon.palette, paletteData.PrimaryVariantColorIndices, paletteData.PrimaryVariantColorIndices2, typeData);
            if(data.VariantTypes.Length > 1)
            {
                var secondVariantType = data.VariantTypes[1];
                typeData = typeColorData[secondVariantType];
                ApplyColorChanges(pokemon.palette, paletteData.SecondaryVariantColorIndices, paletteData.SecondaryVariantColorIndices2, typeData);
            }
        }


        private void ApplyColorsPrimarySecondary(PokemonBaseStats pokemon, VariantData data)
        {
            var firstVariantType = data.VariantTypes[0];
            var typeData = typeColorData[firstVariantType];
            var paletteData = variantPaletteData[pokemon.species];
            if (pokemon.IsSingleTyped)
            {
                ApplyColorChanges(pokemon.palette, paletteData.PrimaryVariantColorIndices, paletteData.PrimaryVariantColorIndices2, typeData);
                ApplyColorChanges(pokemon.palette, paletteData.SecondaryVariantColorIndices, paletteData.SecondaryVariantColorIndices2, typeData);
            }
            else
            {
                if (pokemon.PrimaryType == firstVariantType)
                {
                    ApplyColorChanges(pokemon.palette, paletteData.PrimaryVariantColorIndices, paletteData.PrimaryVariantColorIndices2, typeData);
                }
                else
                {
                    ApplyColorChanges(pokemon.palette, paletteData.SecondaryVariantColorIndices, paletteData.SecondaryVariantColorIndices2, typeData);
                }
                if (data.VariantTypes.Length > 1)
                {
                    var secondVariantType = data.VariantTypes[1];
                    typeData = typeColorData[secondVariantType];
                    if (!typeColorData.ContainsKey(secondVariantType))
                        return;
                    if (pokemon.PrimaryType == secondVariantType)
                    {
                        ApplyColorChanges(pokemon.palette, paletteData.PrimaryVariantColorIndices, paletteData.PrimaryVariantColorIndices2, typeData);
                    }
                    else
                    {
                        ApplyColorChanges(pokemon.palette, paletteData.SecondaryVariantColorIndices, paletteData.SecondaryVariantColorIndices2, typeData);
                    }
                }
            }
        }

        private void ApplyColorChanges(Palette palette, int[] indices, int[] secondaryIndices, TypeColorData typeData)
        {
            foreach (int colorIndex in indices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.GetColor(color.Value);
            }
            foreach (int colorIndex in secondaryIndices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.HasSecondaryColors ? typeData.GetSecondaryColor(color.Value) : typeData.GetColor(color.Value);
            }
        }

        private void ApplyColorChanges(Palette palette, int[] indices, TypeColorData typeData)
        {
            foreach (int colorIndex in indices)
            {
                var color = palette.Colors[colorIndex];
                palette.Colors[colorIndex] = typeData.GetColor(color.Value);
            }
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

            public bool TryGetReplacementType(PokemonType orignalType, out PokemonType replacementType)
            {
                foreach(var replacement in TypeReplacements)
                {
                    if(replacement.originalType == orignalType)
                    {
                        replacementType = replacement.newType;
                        return true;
                    }
                }
                replacementType = PokemonType.Unknown;
                return false;
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

            public Dictionary<Ability, Ability> AbilityReplacements = new Dictionary<Ability, Ability>(3);

            public List<(Move oldMove, Move newMove)> MoveReplacements { get; } = new List<(Move oldMove, Move newMove)>();
            public List<LearnSet.Entry> BonusMoves { get; } = new List<LearnSet.Entry>();
        }

        private class PaletteData
        {
            public int[] PrimaryVariantColorIndices { get; }
            public int[] PrimaryVariantColorIndices2 { get; }
            public int[] SecondaryVariantColorIndices { get; }
            public int[] SecondaryVariantColorIndices2 { get; }

            public PaletteData(int[] primaryColorIndices, int[] secondaryColorIndices = null, int[] primaryColorIndices2 = null, int[] secondaryColorIndices2 = null)
            {
                PrimaryVariantColorIndices = primaryColorIndices;
                PrimaryVariantColorIndices2 = primaryColorIndices2 ?? Array.Empty<int>();
                SecondaryVariantColorIndices = secondaryColorIndices ?? Array.Empty<int>();
                SecondaryVariantColorIndices2 = secondaryColorIndices2 ?? Array.Empty<int>();
            }

        }

        private class TypeColorData
        {
            private const int numColors = 32;
            public bool HasSecondaryColors => SecondaryColors.Length > 0;
            public int ValueOffset { get; set; } = 0;
            private Color[] Colors { get; }
            private Color[] SecondaryColors { get; }
            public TypeColorData(Color baseColor)
            {
                Colors = GenerateColorAtAllValues(baseColor);
                SecondaryColors = Array.Empty<Color>();
            }

            public TypeColorData(Color baseColor, Color secondaryColor)
            {
                Colors = GenerateColorAtAllValues(baseColor);
                SecondaryColors = GenerateColorAtAllValues(secondaryColor);
            }

            public Color GetColor(int value) => GetColor(Colors, value);
            public Color GetSecondaryColor(int value) => GetColor(Colors, value);

            private Color GetColor(Color[] colors, int value)
            {
                return colors[Math.Max(Math.Min(value + ValueOffset, colors.Length - 1), 0)];
            }

            private Color[] GenerateColorAtAllValues(Color baseColor)
            {
                int value = baseColor.Value;
                var colors = new Color[numColors];
                for (int i = 0; i < colors.Length; ++i)
                {
                    int offset = i - value;
                    colors[i] = new Color(baseColor.r + offset, baseColor.g + offset, baseColor.b + offset, baseColor.a);
                    colors[i].Clamp(0, 31);
                }
                return colors;
            }
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
