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
            SecondaryTypeReplacement,
            GainSecondaryType = SecondaryTypeReplacement,
            PrimaryTypeReplacement,
            SingleTypeReplacement = PrimaryTypeReplacement,
            DoubleTypeReplacement,
            TypeLoss
        }
        public enum TypeProfile
        {
            Balanced,
            Special,
            Physical
        }
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        private readonly List<PokemonType> types;
        public PokemonVariantRandomizer(Random rand, IDataTranslator dataT)
        {
            this.rand = rand;
            this.dataT = dataT;
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
                // Log original type data
                OriginalTypes = (pokemon.PrimaryType, pokemon.SecondaryType),
                // Change to variant type
                TransformationType = ChooseType(pokemon, settings),
            };
            ModifyBaseStats(pokemon, settings, variantData);
            // Propogate type (Change to propogate variance)
            PropogateVariance(pokemon, settings, variantData);

            // Modify Evolutions (if applicable)

            // Modify Color Palette

            // Create new signature move (if applicable)

            // Modify Learnset

            // Modify Base Stats (If applicable)
        }

        private void PropogateVariance(PokemonBaseStats pokemon, Settings settings, VariantData data)
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
                    OriginalTypes = (evolvedPokemon.PrimaryType, evolvedPokemon.SecondaryType),
                    TransformationType = PropogateType(pokemon, evolvedPokemon, newTypes, settings, data)
                };
                ModifyBaseStats(evolvedPokemon, settings, evoVariantData);
                // Keep propogating
                PropogateVariance(evolvedPokemon, settings, evoVariantData);
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

        private TypeTransformation PropogateType(PokemonBaseStats pokemon, PokemonBaseStats evolvedPokemon, List<PokemonType> newTypes, Settings settings, VariantData data)
        {
            TypeTransformation newTransformationType = data.TransformationType;
            // If the evolved pokemon is the same type as the base pokemon originally was, just pass the type changes through
            if (evolvedPokemon.PrimaryType == data.OriginalTypes.PrimaryType && evolvedPokemon.SecondaryType == data.OriginalTypes.SecondaryType)
            {
                evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                evolvedPokemon.SecondaryType = pokemon.SecondaryType;
            }
            else if (data.OriginallySingleType) // Pattern is either single replacement, gain type, or double replacement
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
                    }
                    else if (data.TransformationType == TypeTransformation.DoubleTypeReplacement)
                    {
                        // Replace the first type with something that isn't the original single type or either of the new types
                        evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                        // Carry over the newly gained secondary type
                        evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                    }
                }
                else if (evolvedPokemon.IsType(data.OriginalTypes.PrimaryType)) // pokemon gains a type when evolving (e.g shroomish -> breloom)
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
                else if (evolvedPokemon.IsType(data.OriginalTypes.PrimaryType))
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
                else if (evolvedPokemon.IsType(data.OriginalTypes.SecondaryType))
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

        // May also modify EVs
        private void ModifyBaseStats(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            int originalBST = pokemon.BST;
            var oldTypeProfile = ComputeTypeProfile(data.OriginalTypes.PrimaryType, data.OriginalTypes.SecondaryType);
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
                    pokemon.stats[statChoice] = IncreaseStat(pokemon.stats[statChoice], numstats);
                    statGrowth -= increment;
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
            private (PokemonType PrimaryType, PokemonType SecondaryType) originalTypes;
            public (PokemonType PrimaryType, PokemonType SecondaryType) OriginalTypes 
            { 
                get => originalTypes;
                set
                {
                    originalTypes = value;
                    OriginallySingleType = value.PrimaryType == value.SecondaryType;
                } 
            }
            public bool OriginallySingleType { get; private set; }
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
                        _ => Array.Empty<PokemonType>()
                    };
                }
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
