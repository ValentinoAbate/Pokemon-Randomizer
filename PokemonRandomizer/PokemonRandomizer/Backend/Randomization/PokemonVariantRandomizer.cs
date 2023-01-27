using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.Randomization.VariantPaletteModifier;

namespace PokemonRandomizer.Backend.Randomization
{
    public class PokemonVariantRandomizer
    {
        public const string FRLGPalKey = "FRLG";
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
        private readonly IReadOnlyList<PokemonType> types;
        private readonly string paletteKey;
        private readonly HashSet<Move> availableMoves;
        private readonly TypeEffectivenessChart typeChart;
        private readonly VariantPaletteModifier paletteModifier;
        public PokemonVariantRandomizer(Random rand, RomData data, PokemonRandomizer.Settings settings, BonusMoveGenerator bonusMoveGenerator, string paletteKey, VariantPaletteModifier paletteModifier)
        {
            this.rand = rand;
            dataT = data;
            this.bonusMoveGenerator = bonusMoveGenerator;
            this.paletteKey = paletteKey;
            this.paletteModifier = paletteModifier;
            types = data.Types;
            typeChart = data.TypeDefinitions;
            availableMoves = data.GetValidMoves(true, settings.BanSelfdestruct);
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
            if(pokemon.species is Pokemon.CASTFORM)
            {
                pokemon.SecondaryType = RandomSecondaryType(types);
                return TypeTransformation.GainSecondaryType;
            }
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
            else if (pokemon.OriginallySingleTyped) // Pattern is either single replacement, gain type, or double replacement
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
                        newTransformationType = TypeTransformation.DoubleTypeReplacement;
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
                        newTransformationType = TypeTransformation.DoubleTypeReplacement;
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
                WonderGuardFix(evolvedPokemon);
            }
            if (evolvedPokemon.PrimaryType == evolvedPokemon.OriginalPrimaryType && evolvedPokemon.SecondaryType == evolvedPokemon.OriginalSecondaryType)
            {
                Logger.main.Warning($"None transformation type detected for {pokemon.Name} -> {evolvedPokemon.Name}");
                return TypeTransformation.None;
            }
            return newTransformationType;
        }

        private void WonderGuardFix(PokemonBaseStats pokemon)
        {
            // If the pokemon doesn't have wonder guard, return
            if (!pokemon.HasAbility(Ability.Wonder_Guard))
                return;
            // If the pokemon has weaknesses, return
            if (!typeChart.HasNoWeaknesses(pokemon.PrimaryType, pokemon.SecondaryType))
                return;
            // If single typed, try to create a dual type mon with the single type that has weaknesses
            if (pokemon.IsSingleTyped)
            {
                if(TryGetNoWeaknessSecondaryType(pokemon.PrimaryType, out var secondaryType))
                {
                    pokemon.SecondaryType = secondaryType;
                }
            }
            else if (pokemon.IsDualTyped)
            {
                if (!typeChart.HasNoWeaknesses(pokemon.PrimaryType))
                {
                    if (!typeChart.HasNoWeaknesses(pokemon.SecondaryType))
                    {
                        pokemon.SetSingleType(rand.RandomBool() ? pokemon.PrimaryType : pokemon.SecondaryType);
                    }
                    else
                    {
                        pokemon.SetSingleType(pokemon.PrimaryType);
                    }
                }
                else if (!typeChart.HasNoWeaknesses(pokemon.SecondaryType))
                {
                    pokemon.SetSingleType(pokemon.SecondaryType);
                }
                else if (TryGetNoWeaknessSecondaryType(pokemon.PrimaryType, out var secondaryType))
                {
                    pokemon.SecondaryType = secondaryType;
                }
            }
            Logger.main.Info($"{pokemon.Name} had no weaknesses and has Wonder Guard. Correcting type to {pokemon.PrimaryType}/{pokemon.SecondaryType}");
        }

        private bool TryGetNoWeaknessSecondaryType(PokemonType primaryType, out PokemonType secondaryType)
        {
            var typePool = new List<PokemonType>(types.Count);
            foreach(var type in types)
            {
                if(type != primaryType)
                {
                    typePool.Add(type);
                }
            }
            while(typePool.Count > 0)
            {
                secondaryType = rand.Choice(typePool);
                if(!typeChart.HasNoWeaknesses(primaryType, secondaryType))
                {
                    return true;
                }
                typePool.Remove(secondaryType);
            }
            secondaryType = primaryType;
            return false;
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
                // Check again in case the replacement ablity has a replacement
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
                if (!data.AbilityReplacements.ContainsKey(ability))
                {
                    data.AbilityReplacements.Add(ability, newAbility);
                }
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
            Move.TAIL_GLOW,
            Move.NATURE_POWER,
            Move.HIDDEN_POWER,
            Move.WEATHER_BALL,
        };

        // Pokemon that shouldn't recieve bonus moves, only replacement moves
        private static readonly HashSet<Pokemon> limitedLearnsetPokemon = new HashSet<Pokemon>()
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
            return moveData.IsType(type) && effectivePower >= minPower && effectivePower <= maxPower;
        }

        public bool HasAttackingMoveOfType(LearnSet learnSet, PokemonType type, int minPower = 0, int maxPower = 0xFF)
        {
            return learnSet.Any(entry => IsAttackingMoveOfType(entry, type, minPower, maxPower));
        }

        private List<MoveData> GetAvailibleAddMoves(LearnSet learnSet)
        {
            var lookup = learnSet.GetMovesLookup();
            return availableMoves.Where(m => m != Move.None && !lookup.Contains(m)).Select(m => dataT.GetMoveData(m)).ToList();
        }

        private List<MoveData> GetAvailibleTypeMoves(IEnumerable<MoveData> allMoves, PokemonType type, LearnSet learnSet)
        {
            var lookup = learnSet.GetMovesLookup();
            var moves = allMoves.Where(m => m.IsType(type) && !lookup.Contains(m.move)).ToList();
            moves.Sort((m1, m2) => m1.EffectivePower.CompareTo(m2.EffectivePower));
            return moves;
        }

        private LearnSet.Entry[] GetEntriesOfType(PokemonType type, LearnSet learnSet)
        {
            return learnSet.Where(entry => dataT.GetMoveData(entry.move).IsType(type)).ToArray();
        }

        private void SpecialCastformMoves(PokemonBaseStats pokemon, VariantData data)
        {
            if (data.GainedTypes.Contains(PokemonType.RCK) || data.GainedTypes.Contains(PokemonType.GRD) || data.GainedTypes.Contains(PokemonType.STL))
            {
                var lookup = pokemon.learnSet.GetMovesLookup();
                if (!lookup.Contains(Move.SANDSTORM))
                {
                    pokemon.learnSet.Add(Move.SANDSTORM, 20);
                }
            }
            if (data.GainedTypes.Contains(PokemonType.GRS))
            {
                var lookup = pokemon.learnSet.GetMovesLookup();
                if (!lookup.Contains(Move.SOLARBEAM))
                {
                    bonusMoveGenerator.AddBonusMove(pokemon, Move.SOLARBEAM);
                }
            }
            if (data.GainedTypes.Contains(PokemonType.ELE))
            {
                var lookup = pokemon.learnSet.GetMovesLookup();
                if (!lookup.Contains(Move.THUNDER))
                {
                    bonusMoveGenerator.AddBonusMove(pokemon, Move.THUNDER);
                }
            }
            if (data.GainedTypes.Contains(PokemonType.ICE))
            {
                var lookup = pokemon.learnSet.GetMovesLookup();
                if (!lookup.Contains(Move.BLIZZARD))
                {
                    bonusMoveGenerator.AddBonusMove(pokemon, Move.BLIZZARD);
                }
            }
            // Hurricane (where supported)
        }

        private void ModifyLearnsetLimited(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            // Apply standard replacements
            var availableAddMoves = GetAvailibleAddMoves(pokemon.learnSet);
            foreach (var typeReplacement in data.TypeReplacements)
            {
                ApplyStandardReplacements(typeReplacement, availableAddMoves, pokemon, data);
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
            if (limitedLearnsetPokemon.Contains(pokemon.species))
            {
                ModifyLearnsetLimited(pokemon, settings, data);
                return;
            }
            // Apply carried over data from evolutionary line
            foreach (var moveReplacement in data.MoveReplacements)
            {
                var newMoveData = dataT.GetMoveData(moveReplacement.newMove);
                var oldMoveData = dataT.GetMoveData(moveReplacement.oldMove);
                // If the pokemon is not currently the type of the new move and was the type of the old move, don't make the replacement
                // This move will be replaced with a different move later
                if (!newMoveData.IsType(pokemon) && oldMoveData.IsOriginalType(pokemon))
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
                if (!bonusMoveData.IsType(pokemon))
                    continue;
                pokemon.learnSet.Add(bonusMove);
            }
            var availableAddMoves = GetAvailibleAddMoves(pokemon.learnSet);
            // Apply signiture move replacement
            // Replace Types
            foreach (var typeReplacement in data.TypeReplacements)
            {
                var availableTypeMoves = ApplyStandardReplacements(typeReplacement, availableAddMoves, pokemon, data);
                // Pokemon still doesn't have any attacking moves of the replacement type, add some!
                if (!HasAttackingMoveOfType(pokemon.learnSet, typeReplacement.newType, 2, 50))
                {
                    AddMove(pokemon, 2, 50, 1, 18, data, ref availableTypeMoves);
                }
                if (!HasAttackingMoveOfType(pokemon.learnSet, typeReplacement.newType, 51))
                {
                    AddMove(pokemon, 51, 120, 22, 99, data, ref availableTypeMoves);
                }
            }
            foreach (var gainedType in data.GainedTypes)
            {
                if (data.BonusMoves.Any(entry => dataT.GetMoveData(entry.move).IsType(gainedType)))
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
            if(pokemon.species is Pokemon.CASTFORM)
            {
                SpecialCastformMoves(pokemon, data);
            }
        }

        private List<MoveData> ApplyStandardReplacements((PokemonType originalType, PokemonType newType) typeReplacement, IEnumerable<MoveData> availableAddMoves, PokemonBaseStats pokemon, VariantData data)
        {
            var availibleTypeMoves = GetAvailibleTypeMoves(availableAddMoves, typeReplacement.newType, pokemon.learnSet);
            var entriesToReplace = GetEntriesOfType(typeReplacement.originalType, pokemon.learnSet);
            var moveReplacements = new Dictionary<Move, List<LearnSet.Entry>>(entriesToReplace.Length);
            // Construct the move to entries map
            foreach (var entry in entriesToReplace)
            {
                if (!moveReplacements.ContainsKey(entry.move))
                {
                    moveReplacements.Add(entry.move, new List<LearnSet.Entry>() { entry });
                }
                else
                {
                    moveReplacements[entry.move].Add(entry);
                }
            }
            foreach (var kvp in moveReplacements)
            {
                // Dont apply replacement for moves that have multiple override typings if we are still a valid type
                // For example, poisonpowder doesn need to be replaced if oddish becomes grass/fire, and harden doesn't need to be place if metapod becomes rock
                if (dataT.GetMoveData(kvp.Key).IsType(pokemon))
                {
                    continue;
                }
                // Get the new move and set all relevant entries to the new move
                var newMove = ReplaceMove(kvp.Key, data, ref availibleTypeMoves);
                foreach (var entry in kvp.Value)
                {
                    entry.move = newMove;
                }
            }
            return availibleTypeMoves;
        }

        private void AddMove(PokemonBaseStats pokemon, int minPower, int maxPower, int minLevel, int maxLevel, VariantData data, ref List<MoveData> availibleMoves)
        {
            var eligibleMoves = availibleMoves.Where(m => m.EffectivePower >= minPower && m.EffectivePower <= maxPower);
            if (!eligibleMoves.Any())
            {
                Logger.main.Warning($"Attemping to add a variant move to {pokemon.Name} but no eligible move found. Move add will be skipped");
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
                if (eligibleMoves.Any())
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
                if (oldMove.IsType(PokemonType.NRM))
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

        private Move ChooseAttackingMove(IEnumerable<MoveData> allMoves, int oldMovePower, int powerDiffTolerance)
        {
            var nonStatusMoves = allMoves.Where(m => !m.IsStatus && !m.IsOneHitKO);
            var eligibleMoves = nonStatusMoves.Where(m => m.EffectivePower >= oldMovePower - powerDiffTolerance && m.EffectivePower <= oldMovePower + powerDiffTolerance);
            if (eligibleMoves.Any())
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
            if (eligibleMoves.Any())
            {
                return rand.Choice(eligibleMoves).move;
            }
            return Move.None;
        }

        #endregion

        #region Palette

        private static readonly Dictionary<string, Dictionary<Pokemon, PaletteData>> variantPaletteDataOverrides = new Dictionary<string, Dictionary<Pokemon, PaletteData>>() 
        {
            {FRLGPalKey, new Dictionary<Pokemon, PaletteData>()
            {
                { Pokemon.SQUIRTLE, new PaletteData(Range(11, 14), PalRange(2, 3, 4, 6, 7, 8, 9, 10))},
                { Pokemon.WARTORTLE, new PaletteData(Range(11, 14), Range(5, 10))},
                { Pokemon.BLASTOISE, new PaletteData(Range(11, 14), Range(4, 10))},
                { Pokemon.SANDSHREW, new PaletteData(Range(3, 7))},
                { Pokemon.NIDOKING, new PaletteData(PalRange(7, 11, 12, 13, 14), Range(8, 10))},
                { Pokemon.NINETALES, new PaletteData(Range(9, 14), Range(2, 4))},
                { Pokemon.PARASECT, new PaletteData(PalRange(4, 5, 11, 12, 13, 14), Range(6, 9))},
                { Pokemon.BELLSPROUT, new PaletteData(PalRange(1, 6, 7, 8), PalRange(2, 9, 10, 11), null, Range(3, 5))},
                { Pokemon.HYPNO, new PaletteData(Range(1, 5), Range(12, 13))}, // may also want to include 6 in primary. secondary may be unneccessary
                { Pokemon.LICKITUNG, new PaletteData(PalRange(5, 10, 11, 12, 13, 14), Range(6, 8))}, // 5 is shared between belly and tongue
                { Pokemon.STARYU, new PaletteData(PalRange(6, 7, 8, 9, 14), Range(2, 5), Range(10, 13))},
                { Pokemon.MR_MIME, new PaletteData(Range(5, 8), Range(9, 13))},
                { Pokemon.SNORLAX, new PaletteData(Range(1, 4), Range(5, 9), Range(10, 11))},
                { Pokemon.DRATINI, new PaletteData(PalRange(9, 11, 12, 13, 14))},
                { Pokemon.DRAGONITE, new PaletteData(Range(1, 5), Range(6, 8))},
                { Pokemon.MEW, new PaletteData(Range(9, 14), Range(3, 4))},
            }
            }
        };

        private static readonly Dictionary<Pokemon, PaletteData> variantPaletteData = new Dictionary<Pokemon, PaletteData>()
        {
            { Pokemon.BULBASAUR, new PaletteData(Range(2, 5), Range(11, 14)) },
            { Pokemon.IVYSAUR, new PaletteData(PalRange(6, 7, 10, 12), PalRange(2, 8, 9), null, PalRange(3, 4, 5, 13, 14, 15)) },
            { Pokemon.VENUSAUR, new PaletteData(Range(1, 4), PalRange(10, 13, 14), null, PalRange(5, 6, 8, 9)) }, // 1 is a shared outline color
            { Pokemon.CHARMANDER, new PaletteData(Range(9, 12)) }, // 4-8 Body and Flame
            { Pokemon.CHARMELEON, new PaletteData(Range(10, 13)) }, // 4-8 Body and Flame
            { Pokemon.CHARIZARD, new PaletteData(Range(10, 13), PalRange(1, 2, 6, 7, 8), null, Range(3, 5)) },
            { Pokemon.SQUIRTLE, new PaletteData(Range(11, 14), Range(2, 4))},
            { Pokemon.WARTORTLE, new PaletteData(Range(11, 14), Range(5, 7))},
            { Pokemon.BLASTOISE, new PaletteData(Range(11, 14), PalRange(4, 8, 9, 10))},
            { Pokemon.CATERPIE, new PaletteData(Range(9, 12), PalRange(3, 4, 8), null, PalRange(2, 5, 6, 7))},
            { Pokemon.METAPOD, new PaletteData(Range(2, 5))},
            { Pokemon.BUTTERFREE, new PaletteData(Range(4, 7),  Range(14, 15))},
            { Pokemon.WEEDLE, new PaletteData(PalRange(4, 5, 6, 7, 13), Range(2, 3))},
            { Pokemon.KAKUNA, new PaletteData(Range(2, 6))},
            { Pokemon.BEEDRILL, new PaletteData(Range(2, 5), Range(10, 12 ))},
            { Pokemon.PIDGEY, new PaletteData(Range(6, 12), Range(3, 5))}, // Pidgey line doesn't do well with light primary / dark secondary colors
            { Pokemon.PIDGEOTTO, new PaletteData(Range(6, 11), Range(3, 5))},
            { Pokemon.PIDGEOT, new PaletteData(PalRange(6, 7, 8, 11, 12, 13, 14), PalRange(3, 4, 5, 9, 10))},
            { Pokemon.RATTATA, new PaletteData(Range(7, 10))},
            { Pokemon.RATICATE, new PaletteData(Range(8, 11), Range(2, 7))},
            { Pokemon.SPEAROW, new PaletteData(Range(10, 13), PalRange(6, 7, 8, 9, 14), Range(2, 5)) },
            { Pokemon.FEAROW, new PaletteData(PalRange(2, 3, 10, 11, 12, 13), PalRange(6, 7, 8, 9, 14)) },
            { Pokemon.EKANS, new PaletteData(Range(12, 15), Range(6, 9))},
            { Pokemon.ARBOK, new PaletteData(Range(9, 12), Range(5, 7))},
            { Pokemon.PIKACHU, new PaletteData(Range(2, 6), Range(10, 12))}, // , Range(10, 12) // Cheeks
            { Pokemon.RAICHU, new PaletteData(Range(2, 5), Range(6, 12))},
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
            { Pokemon.BELLSPROUT, new PaletteData(Range(6, 8), PalRange(2, 9, 10, 11), null, Range(3, 5))},
            { Pokemon.WEEPINBELL, new PaletteData(Range(6, 8), Range(9, 11), null, Range(2, 5))},
            { Pokemon.VICTREEBEL, new PaletteData(PalRange(2, 6, 7, 8), Range(9, 11), null, Range(4, 5))},
            { Pokemon.TENTACOOL, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.TENTACRUEL, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.GEODUDE, new PaletteData(Range(11, 14))},
            { Pokemon.GRAVELER, new PaletteData(Range(12, 15))},
            { Pokemon.GOLEM, new PaletteData(Range(5, 8), Range(9, 12))},
            { Pokemon.PONYTA, new PaletteData(Range(2, 5), Range(9, 12))}, // 2-5 is body (should likely be 2ndary. 9-12 is flames (hue gradient not value gradient, needs more advanced processing)
            { Pokemon.RAPIDASH, new PaletteData(Range(2, 6), Range(9, 12))}, // 2-6 is body (should likely be 2ndary. 9-12 is flames (hue gradient not value gradient, needs more advanced processing)
            { Pokemon.SLOWPOKE, new PaletteData(Range(10, 14), Range(3, 6))},
            { Pokemon.SLOWBRO, new PaletteData(Range(11, 14), Range(7, 9), null, Range(2, 5))},
            { Pokemon.MAGNEMITE, new PaletteData(Range(11, 14), Range(3, 4), Range(5, 6))},
            { Pokemon.MAGNETON, new PaletteData(Range(11, 14), Range(3, 4), Range(5, 6))},
            { Pokemon.FARFETCHD, new PaletteData(Range(11, 14), Range(3, 4), null, Range(8, 10))}, // 5-6 break and feet
            { Pokemon.DODUO, new PaletteData(Range(5, 8), Range(9, 11))},
            { Pokemon.DODRIO, new PaletteData(Range(1, 3), Range(4, 7), null, Range(8, 10))},
            { Pokemon.SEEL, new PaletteData(PalRange(1, 2, 3, 4, 15), Range(5, 7))},
            { Pokemon.DEWGONG, new PaletteData(PalRange(1, 2, 3, 4, 12, 13, 15))},
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
            { Pokemon.MR_MIME, new PaletteData(Range(5, 8), Range(9, 12))},
            { Pokemon.SCYTHER, new PaletteData(Range(11, 14), Range(7, 10), null, Range(5, 6))},
            { Pokemon.JYNX, new PaletteData(Range(10, 13), Range(6, 8))},
            { Pokemon.ELECTABUZZ, new PaletteData(Range(11, 15))},
            { Pokemon.MAGMAR, new PaletteData(Range(11, 14))}, // 7-10 body 2 and part of flame
            { Pokemon.PINSIR, new PaletteData(Range(11, 14), Range(2, 5))},
            { Pokemon.TAUROS, new PaletteData(Range(7, 10), Range(11, 14), Range(2, 5))},
            { Pokemon.MAGIKARP, new PaletteData(Range(12, 15), Range(6, 8), null, Range(9, 11))},
            { Pokemon.GYARADOS, new PaletteData(Range(4, 7), Range(8, 10), null, Range(11, 13))},
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
            { Pokemon.DRAGONAIR, new PaletteData(Range(11, 14), Range(5, 8))},
            { Pokemon.DRAGONITE, new PaletteData(Range(2, 5), Range(6, 8))},
            { Pokemon.MEWTWO, new PaletteData(PalRange(1, 2, 3, 4, 15), Range(5, 8))},
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
            { Pokemon.SENTRET, new PaletteData(Range(2, 8))}, 
            { Pokemon.FURRET, new PaletteData(Range(2, 8))},
            { Pokemon.HOOTHOOT, new PaletteData(Range(2, 9), Range(11, 12))},
            { Pokemon.NOCTOWL, new PaletteData(PalRange(2, 3, 4, 5, 9, 14), Range(6, 8))},
            { Pokemon.LEDYBA, new PaletteData(Range(2, 5), Range(6, 9))}, // 10-11 eyes
            { Pokemon.LEDIAN, new PaletteData(Range(2, 5), Range(6, 9))}, // 10-11 eyes
            { Pokemon.SPINARAK, new PaletteData(Range(2, 6), Range(7, 9))},
            { Pokemon.ARIADOS, new PaletteData(Range(2, 6), Range(7, 9), null, Range(10, 12))},
            { Pokemon.CROBAT, new PaletteData(Range(2, 5), Range(8, 10))},
            { Pokemon.CHINCHOU, new PaletteData(Range(2, 7), Range(8, 13))},
            { Pokemon.LANTURN, new PaletteData(Range(3, 7), Range(8, 12))},
            { Pokemon.PICHU, new PaletteData(Range(2, 6), Range(9, 11))},
            { Pokemon.CLEFFA, new PaletteData(Range(2, 5), Range(6, 10))},
            { Pokemon.IGGLYBUFF, new PaletteData(Range(2, 7), Range(9, 13))},
            { Pokemon.TOGEPI, new PaletteData(Range(6, 10), Range(11, 12), null, Range(13, 14))},
            { Pokemon.TOGETIC, new PaletteData(PalRange(1, 2, 3, 4, 5, 6, 15), Range(11, 12), null, Range(13, 14))},
            { Pokemon.NATU, new PaletteData(Range(2, 5), Range(9, 11), null, Range(6, 8))},
            { Pokemon.XATU, new PaletteData(Range(2, 5), Range(9, 11), null, Range(6, 8))},
            { Pokemon.MAREEP, new PaletteData(Range(6, 9), Range(2, 5), null, Range(10, 12))},
            { Pokemon.FLAAFFY, new PaletteData(Range(6, 10), Range(2, 5), null, Range(11, 13))},
            { Pokemon.AMPHAROS, new PaletteData(Range(2, 5), Range(11, 14))},
            { Pokemon.BELLOSSOM, new PaletteData(Range(8, 10), Range(2, 4), Range(5, 7), Range(11, 13))},
            { Pokemon.MARILL, new PaletteData(Range(6, 10), Range(12, 14))},
            { Pokemon.AZUMARILL, new PaletteData(Range(6, 9), Range(12, 14))},
            { Pokemon.SUDOWOODO, new PaletteData(Range(2, 5), Range(7, 10), null, Range(11, 13))},
            { Pokemon.POLITOED, new PaletteData(Range(6, 9), Range(2, 5))},
            { Pokemon.HOPPIP, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.SKIPLOOM, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.JUMPLUFF, new PaletteData(Range(2, 5), Range(7, 11), null, Range(14, 15))},
            { Pokemon.AIPOM, new PaletteData(Range(2, 5))}, // Range(6, 10) tail, feet, etc
            { Pokemon.SUNKERN, new PaletteData(Range(9, 12), Range(2, 7))},
            { Pokemon.SUNFLORA, new PaletteData(Range(3, 9), Range(12, 15))},
            { Pokemon.YANMA, new PaletteData(PalRange(1, 3, 4, 5, 10), Range(7, 9))},
            { Pokemon.WOOPER, new PaletteData(Range(2, 6), Range(7, 10))},
            { Pokemon.QUAGSIRE, new PaletteData(PalRange(1, 2, 3, 5, 6, 7, 8), Range(11, 12))},
            { Pokemon.ESPEON, new PaletteData(Range(8, 12), Range(5, 6))},
            { Pokemon.UMBREON, new PaletteData(Range(3, 7), Range(8, 11))},
            { Pokemon.MURKROW, new PaletteData(Range(4, 7), Range(9, 12))},
            { Pokemon.SLOWKING, new PaletteData(Range(4, 7), Range(12, 14), null, Range(1, 3))}, 
            { Pokemon.MISDREAVUS, new PaletteData(Range(2, 5), Range(8, 14))}, 
            { Pokemon.UNOWN, new PaletteData(Range(7, 9))}, 
            { Pokemon.WOBBUFFET, new PaletteData(Range(6, 10))},
            { Pokemon.GIRAFARIG, new PaletteData(Range(7, 10), Range(12, 13))},
            { Pokemon.PINECO, new PaletteData(Range(6, 10), PalRange(12))},
            { Pokemon.FORRETRESS, new PaletteData(Range(3, 7), Range(10, 13))},
            { Pokemon.DUNSPARCE, new PaletteData(Range(10, 14), Range(3, 6))},
            { Pokemon.GLIGAR, new PaletteData(Range(3, 7), Range(8, 11))},
            { Pokemon.STEELIX, new PaletteData(Range(4, 8))},
            { Pokemon.SNUBBULL, new PaletteData(Range(3, 6), Range(9, 13))},
            { Pokemon.GRANBULL, new PaletteData(Range(3, 7))},
            { Pokemon.QWILFISH, new PaletteData(Range(4, 8), PalRange(4, 13, 14))},
            { Pokemon.SCIZOR, new PaletteData(Range(2, 5), Range(7, 9))},
            { Pokemon.SHUCKLE, new PaletteData(Range(11, 14), Range(2, 6))},
            { Pokemon.HERACROSS, new PaletteData(Range(2, 5))},
            { Pokemon.SNEASEL, new PaletteData(Range(2, 5), Range(6, 9))},
            { Pokemon.TEDDIURSA, new PaletteData(Range(2, 6), Range(7, 10))},
            { Pokemon.URSARING, new PaletteData(Range(2, 6), Range(7, 10))},
            { Pokemon.SLUGMA, new PaletteData(Range(2, 6))},
            { Pokemon.MAGCARGO, new PaletteData(Range(2, 10), Range(11, 14))},
            { Pokemon.SWINUB, new PaletteData(Range(2, 7), Range(8, 10))},
            { Pokemon.PILOSWINE, new PaletteData(Range(2, 6), Range(8, 10))},
            { Pokemon.CORSOLA, new PaletteData(Range(6, 10))},
            { Pokemon.REMORAID, new PaletteData(Range(2, 6))},
            { Pokemon.OCTILLERY, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.DELIBIRD, new PaletteData(Range(2, 5))},
            { Pokemon.MANTINE, new PaletteData(Range(2, 5), Range(10, 14), null, Range(7, 9))},
            { Pokemon.SKARMORY, new PaletteData(Range(1, 6), Range(7, 10))},
            { Pokemon.HOUNDOUR, new PaletteData(Range(2, 5), Range(6, 9))}, // 15 is partial outline
            { Pokemon.HOUNDOOM, new PaletteData(Range(2, 5), Range(6, 9))}, // 15 is partial outline
            { Pokemon.KINGDRA, new PaletteData(Range(2, 5), Range(9, 12))},
            { Pokemon.PHANPY, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.DONPHAN, new PaletteData(Range(7, 9), Range(2, 5))},
            { Pokemon.PORYGON2, new PaletteData(Range(2, 5), Range(6, 11))},
            { Pokemon.STANTLER, new PaletteData(Range(2, 5))}, // 70 is horns and other fur
            { Pokemon.SMEARGLE, new PaletteData(PalRange(3, 4, 5, 8, 9), PalRange(2, 10, 11, 12))},
            { Pokemon.TYROGUE, new PaletteData(Range(2, 6), Range(7, 10))},
            { Pokemon.HITMONTOP, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.SMOOCHUM, new PaletteData(PalRange(2, 6, 7, 8), Range(9, 12))}, // 3-5 body
            { Pokemon.ELEKID, new PaletteData(Range(2, 5))},
            { Pokemon.MAGBY, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.MILTANK, new PaletteData(Range(2, 5), Range(6, 9))},
            { Pokemon.BLISSEY, new PaletteData(Range(6, 10))},
            { Pokemon.RAIKOU, new PaletteData(Range(2, 5), Range(6, 9))},
            { Pokemon.ENTEI, new PaletteData(PalRange(2, 3, 4, 5, 9, 10, 11), Range(6, 8))},
            { Pokemon.SUICUNE, new PaletteData(Range(2, 5), Range(6, 9))},
            { Pokemon.LARVITAR, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.PUPITAR, new PaletteData(Range(2, 5))},
            { Pokemon.TYRANITAR, new PaletteData(Range(2, 5), Range(7, 10))},
            { Pokemon.LUGIA, new PaletteData(Range(1, 5), Range(6, 9))},
            { Pokemon.HOーOH, new PaletteData(Range(2, 5), Range(6, 9), null, Range(10, 12))},
            { Pokemon.CELEBI, new PaletteData(Range(2, 6), Range(8, 11))},
            { Pokemon.TREECKO, new PaletteData(Range(11, 14), Range(5, 7), PalRange(4, 8, 9))},
            { Pokemon.GROVYLE, new PaletteData(Range(2, 5), PalRange(6, 7, 8, 11), PalRange(1, 10, 13))},
            { Pokemon.SCEPTILE, new PaletteData(PalRange(8, 11, 12, 13, 14), Range(5, 7))},
            { Pokemon.TORCHIC, new PaletteData(Range(11, 14), Range(5, 8))},
            { Pokemon.COMBUSKEN, new PaletteData(Range(11, 14), Range(1, 8))}, // might need to make all one color
            { Pokemon.BLAZIKEN, new PaletteData(Range(5, 8), Range(9, 14))},
            { Pokemon.MUDKIP, new PaletteData(Range(11, 14), Range(5, 8))},
            { Pokemon.MARSHTOMP, new PaletteData(PalRange(4, 10, 11, 12, 13, 14), Range(5, 8))},
            { Pokemon.SWAMPERT, new PaletteData(Range(11, 13), Range(5, 8), PalRange(3, 4,8, 10, 14))},
            { Pokemon.POOCHYENA, new PaletteData(PalRange(1, 5, 6, 7, 8, 9), PalRange(3, 12, 13))},
            { Pokemon.MIGHTYENA, new PaletteData(Range(3, 9), Range(11, 13))},
            { Pokemon.ZIGZAGOON, new PaletteData(PalRange(1, 2, 3, 4, 6, 7, 14), PalRange(5, 8, 9, 13))},
            { Pokemon.LINOONE, new PaletteData(Range(1, 4), PalRange(5, 7, 8, 9, 10, 13, 14, 15))},
            { Pokemon.WURMPLE, new PaletteData(PalRange(1, 2, 5, 6, 7))},
            { Pokemon.SILCOON, new PaletteData(PalRange(1, 2, 3, 5, 6, 7, 8))},
            { Pokemon.BEAUTIFLY, new PaletteData(Range(7, 11), Range(4, 6), PalRange(13), PalRange(14)) },
            { Pokemon.CASCOON, new PaletteData(PalRange(1, 2, 3, 5, 6, 7, 8))},
            { Pokemon.DUSTOX, new PaletteData(Range(5, 7), Range(11, 12), PalRange(1, 2, 3, 8, 9, 10), Range(13, 14)) },
            { Pokemon.LOTAD, new PaletteData(Range(5, 10), PalRange(4, 11, 12, 13))},
            { Pokemon.LOMBRE, new PaletteData(PalRange(1, 2, 8, 9, 10, 12), Range(4, 6))},
            { Pokemon.LUDICOLO, new PaletteData(Range(1, 5))}, // Other colors are too shared, may be able to come back to this one
            { Pokemon.SEEDOT, new PaletteData(PalRange(1 ,4, 5, 6), PalRange(3, 7, 8, 9))},
            { Pokemon.NUZLEAF, new PaletteData(PalRange(1, 7, 8, 9, 13), Range(10, 12))},
            { Pokemon.SHIFTRY, new PaletteData(Range(4, 6), Range(11, 13))},
            { Pokemon.NINCADA, new PaletteData(PalRange(1, 8, 10, 11, 12, 13, 14), PalRange(5, 6, 9))},
            { Pokemon.NINJASK, new PaletteData(Range(2, 4), Range(5, 8))}, // Range 1 should possibly include index 1, although it affects the wings too
            { Pokemon.SHEDINJA, new PaletteData(PalRange(1, 2, 3, 4, 5, 9, 10), Range(11, 14))},
            { Pokemon.TAILLOW, new PaletteData(Range(1, 4), PalRange(5, 9, 10, 11, 14))},
            { Pokemon.SWELLOW, new PaletteData(Range(1, 4), PalRange(5, 9, 10, 11, 14))},
            { Pokemon.SHROOMISH, new PaletteData(Range(7, 12))}, // 1 - 6 brown colors
            { Pokemon.BRELOOM, new PaletteData(PalRange(7, 9, 10, 11))}, // 2-6, 8, 12, 13, 15 other colors
            { Pokemon.SPINDA, new PaletteData(PalRange(1, 2, 3, 4, 13, 14), Range(5, 10))},
            { Pokemon.WINGULL, new PaletteData(PalRange(6, 10, 11), Range(1, 5))}, // 7-9 beak
            { Pokemon.PELIPPER, new PaletteData(PalRange(6, 7, 8, 15), PalRange(9, 10, 14))}, // 1-5, 12 beak
            { Pokemon.SURSKIT, new PaletteData(Range(1, 4), PalRange(5, 6, 7, 11), null, Range(9, 10))},
            { Pokemon.MASQUERAIN, new PaletteData(Range(1, 4), Range(5, 7), Range(10, 11), Range(12, 13))},
            { Pokemon.WAILMER, new PaletteData(PalRange(2, 3, 4, 10, 11), PalRange(5, 6, 7, 9, 12))},
            { Pokemon.WAILORD, new PaletteData(PalRange(6, 7, 8, 9, 10, 14, 15), PalRange(1, 2, 3, 4, 5, 11, 12))},
            { Pokemon.SKITTY, new PaletteData(PalRange(8, 9, 10, 12, 13, 14, 15), Range(2, 5))}, // maybe only use one color
            { Pokemon.DELCATTY, new PaletteData(PalRange(7, 12, 13, 14, 15), PalRange(2, 3, 4, 5, 6, 8))}, // maybe only use one color
            { Pokemon.KECLEON, new PaletteData(Range(2, 5), Range(10, 14), null, Range(7, 9))},
            { Pokemon.BALTOY, new PaletteData(PalRange(1, 2, 4, 5, 6), Range(7, 8))},
            { Pokemon.CLAYDOL, new PaletteData(Range(1, 4), Range(8, 10), null, Range(11, 12))},
            { Pokemon.NOSEPASS, new PaletteData(Range(3, 6), Range(7, 10))},
            { Pokemon.TORKOAL, new PaletteData(PalRange(4, 5, 6, 7, 11, 12, 13, 14), Range(8, 10))},
            { Pokemon.SABLEYE, new PaletteData(PalRange(1, 2, 3, 4, 9, 13), PalRange(5, 6, 7, 10, 11, 12), null, PalRange(8, 15))},
            { Pokemon.BARBOACH, new PaletteData(Range(3, 6), PalRange(8, 10, 11, 12, 13, 14))},
            { Pokemon.WHISCASH, new PaletteData(Range(2, 5), PalRange(8, 9, 11, 12, 13, 14))},
            { Pokemon.LUVDISC, new PaletteData(PalRange(1, 2, 5, 6, 7, 8))},
            { Pokemon.CORPHISH, new PaletteData(PalRange(2, 3, 4, 5, 10, 11, 12), Range(6, 9))},
            { Pokemon.CRAWDAUNT, new PaletteData(Range(2, 10), Range(11, 14))},
            { Pokemon.FEEBAS, new PaletteData(PalRange(1, 2, 3, 4, 7), Range(10, 15), null, Range(8, 9))},
            { Pokemon.MILOTIC, new PaletteData(Range(1, 5), Range(11, 13), Range(8, 10), Range(6, 7))}, // PALETTE: Might need to recude marked colors
            { Pokemon.CARVANHA, new PaletteData(PalRange(1, 2, 4, 5, 6), PalRange(3, 10, 11, 12))}, // PalRange(7, 8, 9, 13) belly
            { Pokemon.SHARPEDO, new PaletteData(Range(1, 4), Range(6, 7))}, // 10-12 fins
            { Pokemon.TRAPINCH, new PaletteData(PalRange(1, 2, 3, 4, 12), Range(6, 8))},
            { Pokemon.VIBRAVA, new PaletteData(Range(1, 3), PalRange(4, 5, 6, 10))},
            { Pokemon.FLYGON, new PaletteData(Range(5, 7), PalRange(1, 3, 4, 8, 9, 10), null, PalRange(2, 11, 12, 13))}, // not sure which colors should be which here
            { Pokemon.MAKUHITA, new PaletteData(PalRange(5, 6, 7, 12, 13), Range(1, 4))},
            { Pokemon.HARIYAMA, new PaletteData(Range(1, 4), Range(5, 8), Range(11, 13), Range(9, 10))},
            { Pokemon.ELECTRIKE, new PaletteData(Range(1, 4), Range(5, 7))},
            { Pokemon.MANECTRIC, new PaletteData(Range(1, 4), Range(5, 8))},
            { Pokemon.NUMEL, new PaletteData(Range(1, 4), Range(8, 12), null, Range(5, 7))},
            { Pokemon.CAMERUPT, new PaletteData(Range(2, 5), Range(10, 14))},
            { Pokemon.SPHEAL, new PaletteData(PalRange(2, 3, 4, 5, 12), Range(7, 11))},
            { Pokemon.SEALEO, new PaletteData(PalRange(1, 2, 3, 4, 12, 13), PalRange(9, 10, 11, 14))},
            { Pokemon.WALREIN, new PaletteData(PalRange(1, 2, 3, 4, 11), Range(5, 8))}, // May take color 2 out for this whole fam
            { Pokemon.CACNEA, new PaletteData(Range(1, 5), Range(6, 10), null, Range(12, 14))},
            { Pokemon.CACTURNE, new PaletteData(PalRange(1, 2, 3, 4, 11), Range(5, 10))},
            { Pokemon.SNORUNT, new PaletteData(Range(5, 7), Range(2, 4))},
            { Pokemon.GLALIE, new PaletteData(Range(1, 6), Range(7, 9))},
            { Pokemon.LUNATONE, new PaletteData(Range(1, 10), Range(11, 14))},
            { Pokemon.SOLROCK, new PaletteData(PalRange(1, 2, 3, 4, 9, 10), PalRange(5, 6, 7, 8, 11))},
            { Pokemon.AZURILL, new PaletteData(PalRange(1, 2, 3, 4, 5, 8, 11, 12), PalRange(6, 7, 9))},
            { Pokemon.SPOINK, new PaletteData(PalRange(1, 2, 3, 4, 12), Range(5, 9))},
            { Pokemon.GRUMPIG, new PaletteData(PalRange(8, 9, 10, 11, 13), PalRange(5, 6, 7, 12))},
            { Pokemon.PLUSLE, new PaletteData(Range(1, 5), Range(6, 8))},
            { Pokemon.MINUN, new PaletteData(Range(1, 5), Range(6, 8))},
            { Pokemon.MAWILE, new PaletteData(Range(12, 15), Range(7, 10))}, // 3-5 tongue
            { Pokemon.MEDITITE, new PaletteData(Range(12, 15), PalRange(6))},
            { Pokemon.MEDICHAM, new PaletteData(Range(2, 5), Range(10, 11))},
            { Pokemon.SWABLU, new PaletteData(PalRange(4, 8, 9, 10, 11, 12, 13), PalRange(1, 5, 7, 14, 15))},
            { Pokemon.ALTARIA, new PaletteData(PalRange(7, 8, 9, 10, 14), PalRange(2, 3, 11, 12, 13))},
            { Pokemon.WYNAUT, new PaletteData(PalRange(1, 2, 3, 4, 12))}, // 5, 6, 13 Mouth
            { Pokemon.DUSKULL, new PaletteData(PalRange(1, 2, 3, 4, 11), Range(5, 9))}, // 5, 6, 13 Mouth
            { Pokemon.DUSCLOPS, new PaletteData(PalRange(1, 2, 4, 5, 6), Range(7, 9))}, // 5, 6, 13 Mouth
            { Pokemon.ROSELIA, new PaletteData(PalRange(2, 5, 7, 8), Range(10, 12), null, Range(13, 15))}, // 5, 6, 13 Mouth
            { Pokemon.SLAKOTH, new PaletteData(PalRange(4, 13, 14, 15), Range(9, 12))},
            { Pokemon.VIGOROTH, new PaletteData(PalRange(6, 7, 8, 9, 12), Range(10, 11))},
            { Pokemon.GULPIN, new PaletteData(Range(1, 5), Range(9, 12))},
            { Pokemon.SWALOT, new PaletteData(PalRange(1, 2, 3, 4, 14), PalRange(10, 11, 13))},
            { Pokemon.TROPIUS, new PaletteData(PalRange(1, 2, 3, 4, 15), Range(5, 8), null, Range(9, 10))},
            { Pokemon.WHISMUR, new PaletteData(Range(2, 6), Range(8, 11))},
            { Pokemon.LOUDRED, new PaletteData(Range(2, 5), Range(6, 8), null, Range(9, 12))},
            { Pokemon.EXPLOUD, new PaletteData(Range(2, 5), Range(6, 8), null, Range(9, 12))},
            { Pokemon.CLAMPERL, new PaletteData(Range(8, 11), Range(13, 15), null, Range(4, 7))},
            { Pokemon.HUNTAIL, new PaletteData(Range(5, 9), Range(2, 4))},
            { Pokemon.GOREBYSS, new PaletteData(Range(4, 9), PalRange(9, 10, 13, 14, 15))},
            { Pokemon.ABSOL, new PaletteData(Range(1, 4), Range(11, 14))},
            { Pokemon.SHUPPET, new PaletteData(Range(1, 5), Range(6, 7), null, Range(8, 9))},
            { Pokemon.BANETTE, new PaletteData(Range(11, 14))},
            { Pokemon.SEVIPER, new PaletteData(PalRange(1, 3, 4, 9), PalRange(7, 8, 15), PalRange(5, 14))},
            { Pokemon.ZANGOOSE, new PaletteData(PalRange(1, 2, 3, 4, 5, 12), Range(6, 9), null, PalRange(10, 11, 14, 15))},
            { Pokemon.RELICANTH, new PaletteData(PalRange(1, 2, 3, 4, 5, 14), PalRange(6, 8, 9, 10, 11))},
            { Pokemon.ARON, new PaletteData(Range(6, 10), PalRange(14))},
            { Pokemon.LAIRON, new PaletteData(Range(6, 10), PalRange(14))},
            { Pokemon.AGGRON, new PaletteData(Range(6, 10), PalRange(14))},
            { Pokemon.CASTFORM, new PaletteData(PalRange(1, 2, 3, 4, 12))}, // 16-31 is Sun form, 32 - 47 is Rain form, 48 - 63 is snow form (located below). 6-7 visor.
            { Pokemon.VOLBEAT, new PaletteData(Range(5, 8), Range(12, 14), Range(9, 11))},
            { Pokemon.ILLUMISE, new PaletteData(Range(5, 8), Range(12, 14), Range(9, 11))},
            { Pokemon.LILEEP, new PaletteData(Range(2, 5), PalRange(1, 6, 7, 8))}, // 9-11 eyes
            { Pokemon.CRADILY, new PaletteData(Range(2, 5), PalRange(6, 7, 8, 14))}, // 9-11 eyes
            { Pokemon.ANORITH, new PaletteData(Range(2, 5), Range(9, 11))}, // 6-8 highlights
            { Pokemon.ARMALDO, new PaletteData(PalRange(2, 3, 4, 5, 9), Range(10, 11))}, // 6-8 highlights
            { Pokemon.RALTS, new PaletteData(Range(11, 14), Range(5, 8))}, // 1-4 dress
            { Pokemon.KIRLIA, new PaletteData(Range(11, 14), Range(5, 8))}, // 1-4 dress
            { Pokemon.GARDEVOIR, new PaletteData(Range(11, 14), Range(5, 8))}, // 1-4 dress
            { Pokemon.BAGON, new PaletteData(Range(1, 4), Range(8, 10))},
            { Pokemon.SHELGON, new PaletteData(Range(1, 4), PalRange(8, 9, 10, 11, 14))}, // may need to switch
            { Pokemon.SALAMENCE, new PaletteData(Range(1, 4), Range(5, 8))},
            { Pokemon.BELDUM, new PaletteData(PalRange(9, 11, 12, 13, 14))}, // 1, 2, 3, 4, 10 claws
            { Pokemon.METANG, new PaletteData(PalRange(9, 11, 12, 13, 14))}, // 1-5 claws
            { Pokemon.METAGROSS, new PaletteData(Range(9, 14))}, // 1-5 claws
            { Pokemon.REGIROCK, new PaletteData(Range(1, 10), Range(13, 14))},
            { Pokemon.REGICE, new PaletteData(Range(1, 7), Range(12, 14))},
            { Pokemon.REGISTEEL, new PaletteData(PalRange(1, 2, 3, 4, 5, 11), Range(6, 8), null, Range(12, 14))},
            { Pokemon.KYOGRE, new PaletteData(PalRange(5, 9, 11, 12, 13, 14), Range(6, 8))},
            { Pokemon.GROUDON, new PaletteData(PalRange(1, 5, 6, 7, 8, 12), Range(2, 4))}, // may take 2nd color out
            { Pokemon.RAYQUAZA, new PaletteData(Range(1, 5), Range(13, 14), null, Range(9, 10))},
            { Pokemon.LATIAS, new PaletteData(PalRange(9, 11, 12, 13, 14), Range(1, 4))},
            { Pokemon.LATIOS, new PaletteData(PalRange(9, 11, 12, 13, 14), Range(1, 5))},
            { Pokemon.JIRACHI, new PaletteData(Range(10, 13), PalRange(3, 5, 6))},
            { Pokemon.DEOXYS, new PaletteData(Range(1, 4), Range(5, 7))}, // May need to add body (12)
            { Pokemon.CHIMECHO, new PaletteData(PalRange(3, 4, 5, 10), PalRange(2, 11, 12, 13))},
        };

        // Castform is all in one big palette. 0-15 is normal form (located above) 16-31 is Sun form, 32 - 47 is Rain form, and 48 - 63 is snow form.
        private static readonly PaletteData castFormSunPalData = new PaletteData(PalRange(17, 18, 19, 20, 24, 25, 26)); // 22-23 visor.
        private static readonly PaletteData castFormRainPalData = new PaletteData(PalRange(33, 34, 35, 36, 40, 41, 42, 47)); // 38-39 visor. 42 - 47 secondary primary?
        private static readonly PaletteData castFormSnowPalData = new PaletteData(PalRange(49, 56, 57, 59, 60)); // 50, 51, 54, 55, 58, 61 secondary primary?

        private PaletteData GetPaletteData(Pokemon pokemon)
        {
            if (variantPaletteDataOverrides.ContainsKey(paletteKey))
            {
                var overrides = variantPaletteDataOverrides[paletteKey];
                if (overrides.ContainsKey(pokemon))
                {
                    return overrides[pokemon];
                }
            }
            return variantPaletteData.ContainsKey(pokemon) ? variantPaletteData[pokemon] : null;
        }

        private void ModifyPalette(PokemonBaseStats pokemon, Settings settings, VariantData data)
        {
            if (data.TransformationType == TypeTransformation.None)
                return;
            var paletteData = GetPaletteData(pokemon.species);
            paletteModifier.ModifyPalette(pokemon.palette, paletteData, data.VariantTypes);
            // Special castform handling.
            if (pokemon.species == Pokemon.CASTFORM && data.VariantTypes.Length > 0)
            {
                var variantType = data.VariantTypes[0];
                if (variantType is not PokemonType.FIR)
                {
                    paletteModifier.ModifyPalette(pokemon.palette, castFormSunPalData, data.VariantTypes);
                }
                if (variantType is not PokemonType.WAT)
                {
                    paletteModifier.ModifyPalette(pokemon.palette, castFormRainPalData, data.VariantTypes);
                }
                if (variantType is not PokemonType.ICE)
                {
                    paletteModifier.ModifyPalette(pokemon.palette, castFormSnowPalData, data.VariantTypes);
                }
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
        private static bool IsSpecial(PokemonType type)
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
                    if (pokemon.OriginallySingleTyped)
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
                    if (!pokemon.OriginallySingleTyped || pokemon.IsSingleTyped)
                        return Enumerable.Empty<PokemonType>();
                    return new PokemonType[] { TransformationType == TypeTransformation.GainPrimaryType ? pokemon.PrimaryType : pokemon.SecondaryType };
                }
            }

            public int[] BonusStats { get; set; } = null;

            public Dictionary<Ability, Ability> AbilityReplacements = new Dictionary<Ability, Ability>(3);

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
