using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Randomization
{
    internal class ScriptRandomizer
    {
        private readonly Random rand;
        private readonly PkmnRandomizer pokeRand;
        private readonly ItemRandomizer itemRand;
        private readonly List<Action> delayedRandomizationCalls;
        private readonly IDataTranslator dataT;

        public ScriptRandomizer(Random rand, PkmnRandomizer pokeRand, ItemRandomizer itemRand, IDataTranslator dataT, List<Action> delayedRandomizationCalls)
        {
            this.rand = rand;
            this.pokeRand = pokeRand;
            this.itemRand = itemRand;
            this.dataT = dataT;
            this.delayedRandomizationCalls = delayedRandomizationCalls;
        }

        public void RandomizeScript(Script script, Settings settings, Args args)
        {
            foreach (var command in script)
            {
                switch (command)
                {
                    case GotoCommand @goto:
                        RandomizeScript(@goto.script, settings, args);
                        break;
                    case TrainerBattleCommand trainerBattleCommand:
                        if (args.IsGym)
                        {
                            var battle = dataT.GetTrainer(trainerBattleCommand.trainerIndex);
                            battle.GymMetadata = args.gymMetadata;
                            if (battle.IsGymLeader)
                            {
                                args.gymMetadata.Leaders.Add(battle);
                            }
                            else
                            {
                                args.gymMetadata.GymTrainers.Add(battle);
                            }
                        }
                        if (trainerBattleCommand.postBattleScript != null)
                        {
                            RandomizeScript(trainerBattleCommand.postBattleScript, settings, args);
                        }
                        break;
                    case GivePokedexCommand givePokedex:
                        if (settings.StartWithNationalDex)
                        {
                            givePokedex.Type = GivePokedexCommand.PokedexType.National;
                        }
                        break;
                    case GiveItemCommand giveItem:
                        if (giveItem.type == GiveItemCommand.Type.Normal && rand.RollSuccess(settings.FieldItemRandChance))
                        {
                            delayedRandomizationCalls.Add(() => giveItem.item = itemRand.RandomItem(args.items, giveItem.item, settings.FieldItemSettings));
                        }
                        break;
                    case GivePokemonCommand givePokemon:
                        if (givePokemon.type == GivePokemonCommand.Type.Normal && rand.RollSuccess(settings.GiftPokemonRandChance))
                        {
                            // Should choose from fossil set?
                            bool fossil = settings.EnsureFossilRevivesAreFossilPokemon && args.fossilSet.Count > 0 && givePokemon.pokemon.IsFossil();
                            givePokemon.pokemon = pokeRand.RandomPokemon(fossil ? args.fossilSet : args.pokemonSet, givePokemon.pokemon, settings.GiftSpeciesSettings, givePokemon.level);

                        }
                        break;
                    case GiveEggCommand giveEgg:
                        if (rand.RollSuccess(settings.GiftPokemonRandChance))
                        {
                            bool baby = settings.EnsureGiftEggsAreBabyPokemon && args.babySet.Count > 0;
                            giveEgg.pokemon = pokeRand.RandomPokemon(baby ? args.babySet : args.pokemonSet, giveEgg.pokemon, settings.GiftSpeciesSettings, 1);
                        }
                        break;
                    case ShopCommand shopCommand:
                        if (settings.AddCustomItemToPokemarts && settings.CustomMartItem != Item.None && shopCommand.shop.items.Any(i => i == Item.Potion || i.IsPokeBall()))
                        {
                            shopCommand.shop.items.Add(settings.CustomMartItem);
                        }
                        break;
                }
            }
        }

        public class Args
        {
            public IEnumerable<ItemData> items;
            public IEnumerable<Pokemon> pokemonSet;
            public HashSet<Pokemon> fossilSet;
            public HashSet<Pokemon> babySet;
            public GymMetadata gymMetadata;
            public bool IsGym => gymMetadata != null;
        }
    }
}
