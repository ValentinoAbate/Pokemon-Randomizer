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
            SecondaryTypeReplacement,
            GainSecondaryType = SecondaryTypeReplacement,
            PrimaryTypeReplacement,
            SingleTypeReplacement = PrimaryTypeReplacement,
            DoubleTypeReplacement,
            TypeLoss
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
            // Original Type Data
            var origninalTypes = (pokemon.PrimaryType, pokemon.SecondaryType);
            bool originallySingleType = pokemon.IsSingleTyped;
            // Change to variant type
            ChooseType(pokemon, settings, out TypeTransformation transformationType);
            // Propogate type
            PropogateType(pokemon, settings, origninalTypes, originallySingleType, transformationType);

            // Modify Evolutions (if applicable)

            // Modify Color Palette

            // Create new signature move (if applicable)

            // Modify Learnset

            // Modify Base Stats (If applicable)
        }

        private void ChooseType(PokemonBaseStats pokemon, Settings settings, out TypeTransformation transformationType)
        {
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
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.SecondaryType);
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
                    pokemon.PrimaryType = RandomPrimaryType(types, pokemon.PrimaryType);
                    pokemon.SecondaryType = RandomSecondaryType(types, originalPrimaryType, pokemon.PrimaryType, pokemon.SecondaryType);
                }
                else // Type loss
                {
                    pokemon.SetSingleType(RandomPrimaryType(types, pokemon.types));
                }
                if (settings.SafeWonderGuard)
                {
                    WonderGuardFix(pokemon);
                }
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

        private void PropogateType(PokemonBaseStats pokemon, Settings settings, (PokemonType PrimaryType, PokemonType SecondaryType) originalTypes, bool originallySingleType, TypeTransformation transformationType)
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
                // If the evolved pokemon is the same type as the base pokemon originally was, just pass the type changes through
                if(evolvedPokemon.PrimaryType == originalTypes.PrimaryType && evolvedPokemon.SecondaryType == originalTypes.SecondaryType)
                {
                    evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                    evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                    PropogateType(evolvedPokemon, settings, originalTypes, originallySingleType, transformationType);
                    if (settings.SafeWonderGuard)
                    {
                        WonderGuardFix(pokemon);
                    }
                    continue;
                }
                // This evolution is a type change, need to do something more complicated
                // Original Type Data
                var evoOrigninalTypes = (evolvedPokemon.PrimaryType, evolvedPokemon.SecondaryType);
                bool evoOriginallySingleType = evolvedPokemon.IsSingleTyped;
                var newTransformationType = transformationType;
                // Modify the evolved pokemon's type
                if (originallySingleType) // Pattern is either single replacement, gain type, or double replacement
                {
                    // base pokemon was single typed, and evolved pokemon is a different single type (single replacement)
                    // E.g azurill -> marill
                    if (evolvedPokemon.IsSingleTyped)
                    {
                        // Pokemon is still single typed: choose a different primary type that is not the base pokemon's new type or the evolved pokemon's old type
                        if (transformationType == TypeTransformation.SingleTypeReplacement)
                        {
                            evolvedPokemon.SetSingleType(RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType));
                        }
                        else if(transformationType == TypeTransformation.GainSecondaryType)
                        {
                            // Base pokemon gained a dual type, propagate the dual type to the evolved pokemon
                            evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        }
                        else if(transformationType == TypeTransformation.DoubleTypeReplacement)
                        {
                            // Replace the first type with something that isn't the original single type or either of the new types
                            evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                            // Carry over the newly gained secondary type
                            evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        }
                    }
                    else if(evolvedPokemon.IsType(originalTypes.PrimaryType)) // pokemon gains a type when evolving (e.g shroomish -> breloom)
                    {
                        if(transformationType == TypeTransformation.SingleTypeReplacement)
                        {
                            // Carry over the new primary type
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                            transformationType = TypeTransformation.PrimaryTypeReplacement;
                        }
                        else if(transformationType == TypeTransformation.GainSecondaryType)
                        {
                            // Keep the secondary type from the evolution and the new secondary type from the base pokemon
                            if(pokemon.SecondaryType == evolvedPokemon.SecondaryType)
                            {
                                // Evolved pokemon is the same type as the base pokemon. Do nothing (maybe add something later)
                            }
                            else if(pokemon.SecondaryType == PokemonType.FLY)
                            {
                                // If the secondary type was flying, reverse the type order
                                evolvedPokemon.PrimaryType = evolvedPokemon.SecondaryType;
                                evolvedPokemon.SecondaryType = PokemonType.FLY;
                                transformationType = TypeTransformation.SecondaryTypeReplacement;
                            }
                            else
                            {
                                evolvedPokemon.PrimaryType = pokemon.SecondaryType;
                                transformationType = TypeTransformation.PrimaryTypeReplacement;
                            }
                        }
                        else if(transformationType == TypeTransformation.DoubleTypeReplacement)
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
                        transformationType = TypeTransformation.SingleTypeReplacement;
                        if (transformationType == TypeTransformation.PrimaryTypeReplacement || transformationType == TypeTransformation.DoubleTypeReplacement)
                        {
                            evolvedPokemon.SetSingleType(pokemon.PrimaryType);
                        }
                        else if(transformationType == TypeTransformation.SecondaryTypeReplacement)
                        {
                            evolvedPokemon.SetSingleType(pokemon.SecondaryType);
                        }
                        else // Type Loss
                        {
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                            evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                            transformationType = TypeTransformation.GainSecondaryType;
                        }
                    }
                    else if(evolvedPokemon.IsType(originalTypes.PrimaryType))
                    {
                        // Secondary type replacement
                        // E.g nincada -> ninjask, nincada -> shedinja
                        if (transformationType == TypeTransformation.PrimaryTypeReplacement)
                        {
                            // Carry over new primary type
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                        }
                        else if(transformationType == TypeTransformation.SecondaryTypeReplacement)
                        {
                            // Generate new secondary type
                            evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                        }
                        else if(transformationType == TypeTransformation.DoubleTypeReplacement)
                        {
                            // Carry over new primary type and generate new secondary type
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                            evolvedPokemon.SecondaryType = RandomSecondaryTypeAndRemove(newTypes, evolvedPokemon.SecondaryType);
                            transformationType = TypeTransformation.SecondaryTypeReplacement;
                        }
                        else // type loss
                        {
                            evolvedPokemon.PrimaryType = pokemon.PrimaryType;
                            if (!evolvedPokemon.IsSingleTyped)
                                transformationType = TypeTransformation.PrimaryTypeReplacement;
                        }
                    }
                    else if (evolvedPokemon.IsType(originalTypes.SecondaryType))
                    {
                        // Primary type replacement
                        // E.g swablu -> altaria
                        if (transformationType == TypeTransformation.PrimaryTypeReplacement)
                        {
                            // Generate new primary type
                            evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                        }
                        else if (transformationType == TypeTransformation.SecondaryTypeReplacement)
                        {
                            // Carry over secondary type
                            evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                        }
                        else if (transformationType == TypeTransformation.DoubleTypeReplacement)
                        {

                            // Carry over new secondary type and generate new primary type
                            evolvedPokemon.PrimaryType = RandomPrimaryTypeAndRemove(newTypes, evolvedPokemon.PrimaryType);
                            evolvedPokemon.SecondaryType = pokemon.SecondaryType;
                            transformationType = TypeTransformation.PrimaryTypeReplacement;
                        }
                        else // type loss
                        {
                            if (pokemon.PrimaryType == PokemonType.NRM)
                            {
                                // If the secondary type was flying, reverse the type order
                                evolvedPokemon.SecondaryType = evolvedPokemon.PrimaryType;
                                evolvedPokemon.PrimaryType = PokemonType.NRM;
                                transformationType = TypeTransformation.PrimaryTypeReplacement;
                            }
                            else
                            {
                                evolvedPokemon.SecondaryType = pokemon.PrimaryType;
                                transformationType = TypeTransformation.SecondaryTypeReplacement;
                            }
                            if (evolvedPokemon.IsSingleTyped)
                                transformationType = TypeTransformation.TypeLoss;
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
                if (settings.SafeWonderGuard)
                {
                    WonderGuardFix(pokemon);
                }
                // Keep propogating
                PropogateType(evolvedPokemon, settings, evoOrigninalTypes, evoOriginallySingleType, newTransformationType);
            }
        }

        private void WonderGuardFix(PokemonBaseStats pokemon)
        {
            if (!pokemon.abilities.Contains(Ability.Wonder_Guard))
                return;
            if(pokemon.IsType(PokemonType.DRK) && pokemon.IsType(PokemonType.GHO))
            {
                Logger.main.Info($"{pokemon.Name} is DRK/GHO and has Wonder Guard. Correcting type to GHO or DRK");
                pokemon.SetSingleType(rand.RandomBool() ? PokemonType.DRK : PokemonType.GHO);
            }
        }

        public class Settings
        {
            public WeightedSet<TypeTransformation> SingleTypeTransformationWeights { get; set; }
            public WeightedSet<TypeTransformation> DualTypeTransformationWeights { get; set; }
            public bool InvertChanceOfSecondaryTypeChangingForFlyingTypes { get; set; }
            public bool SafeWonderGuard { get; set; } = true;
        }
    }
}
