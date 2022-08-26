﻿using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using PokemonRandomizer.Backend.DataStructures.Trainers;

namespace PokemonRandomizer.Backend.Randomization
{
    internal class ScriptRandomizer
    {
        private readonly Random rand;
        private readonly PkmnRandomizer pokeRand;
        private readonly ItemRandomizer itemRand;
        private readonly List<Action> delayedRandomizationCalls;
        private readonly IDataTranslator dataT;
        private readonly Dictionary<Item, List<ItemCommand>> itemMap = new(4);
        private readonly Dictionary<Pokemon, List<PokemonCommand>> staticPokemonMap = new(4);

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
            itemMap.Clear();
            staticPokemonMap.Clear();
            RandomizeScript(script, settings, args, itemMap);
            foreach(var kvp in itemMap)
            {
                delayedRandomizationCalls.Add(() => RemapItems(kvp.Key, kvp.Value, settings, args));
            }
            RemapStaticPokemon(settings, args);
        }

        private void RemapItems(Item oldItem, IEnumerable<ItemCommand> commands, Settings settings, Args args)
        {
            // No item sources, no need to randomize
            if (!commands.Any(i => i.IsItemSource))
                return;
            // This script is going to be giving an item. Roll randomization
            if (!rand.RollSuccess(settings.FieldItemRandChance))
                return;
            // Remap item commands
            var newItem = itemRand.RandomItem(args.items, oldItem, settings.FieldItemSettings);
            foreach(var command in commands)
            {
                command.Item = newItem;
            }
        }

        private void RemapStaticPokemon(Settings settings, Args args)
        {
            foreach (var (pokemon, commands) in staticPokemonMap)
            {
                // No  sources, no need to randomize
                if (!commands.Any(i => i.IsSource))
                    continue;
                var newPokemon = RandomizeStaticPokemon(pokemon, settings, args);
                if (newPokemon == pokemon)
                    continue;
                foreach(var command in commands)
                {
                    command.Pokemon = newPokemon;
                }
            }
        }

        private void RandomizeScript(Script script, Settings settings, Args args, Dictionary<Item, List<ItemCommand>> itemMap)
        {
            if (script == null)
                return;
            foreach (var command in script)
            {
                switch (command)
                {
                    case GotoCommand @goto:
                        RandomizeScript(@goto.script, settings, args, itemMap);
                        break;
                    case CallCommand call:
                        RandomizeScript(call.script, settings, args, itemMap);
                        break;
                    case TrainerBattleCommand trainerBattleCommand:
                        if (args.IsGym)
                        {
                            var battle = dataT.GetTrainer(trainerBattleCommand.trainerIndex);
                            if (battle.TrainerCategory == Trainer.Category.GymLeader)
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
                            RandomizeScript(trainerBattleCommand.postBattleScript, settings, args, itemMap);
                        }
                        break;
                    case GivePokedexCommand givePokedex:
                        if (settings.StartWithNationalDex)
                        {
                            givePokedex.Type = GivePokedexCommand.PokedexType.National;
                        }
                        break;
                    case ItemCommand itemCommand:
                        if(itemCommand.InputType == CommandInputType.Normal)
                        {
                            itemMap.AddOrAppend(itemCommand.Item, itemCommand);
                        }
                        break;
                    case GivePokemonCommand givePokemon:
                        if (givePokemon.InputType == CommandInputType.Normal && rand.RollSuccess(settings.GiftPokemonRandChance))
                        {
                            // Should choose from fossil set?
                            bool fossil = settings.EnsureFossilRevivesAreFossilPokemon && args.fossilSet.Count > 0 && givePokemon.pokemon.IsFossil();
                            givePokemon.pokemon = pokeRand.RandomPokemon(fossil ? args.fossilSet : args.pokemonSet, givePokemon.pokemon, settings.GiftSpeciesSettings, givePokemon.level);

                        }
                        break;
                    case SetWildBattleCommand setWildBattle:
                        if (setWildBattle.InputType == CommandInputType.Normal)
                        {
                            staticPokemonMap.AddOrAppend(setWildBattle.Pokemon, setWildBattle);
                        }
                        break;
                    case CryCommand cryCommand:
                        if(cryCommand.InputType == CommandInputType.Normal)
                        {
                            staticPokemonMap.AddOrAppend(cryCommand.Pokemon, cryCommand);
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

        private Pokemon RandomizeStaticPokemon(Pokemon pokemon, Settings settings, Args args)
        {
            // Remap if in map
            if (settings.RemapStaticEncounters && args.staticPokemonMap.ContainsKey(pokemon))
            {
                return args.staticPokemonMap[pokemon];
            }
            // Roll randomization
            if (!rand.RollSuccess(settings.StaticEncounterRandChance))
            {
                // Add failure to map
                if (settings.RemapStaticEncounters)
                {
                    args.staticPokemonMap.Add(pokemon, pokemon);
                }
                return pokemon;
            }
            bool isLegendary = PokemonUtils.IsLegendary(pokemon);
            // Skip Legendaries Logic
            if (isLegendary && settings.StaticLegendaryRandomizationStrategy == Settings.LegendaryRandSetting.DontRandomize)
            {
                // Add failure to map
                if (settings.RemapStaticEncounters)
                {
                    args.staticPokemonMap.Add(pokemon, pokemon);
                }
                return pokemon;
            }
            IEnumerable<Pokemon> restrictedPokemon;
            Settings.PokemonSettings pokemonSettings;
            // Keep Legendaries Logic
            if(isLegendary && settings.StaticLegendaryRandomizationStrategy == Settings.LegendaryRandSetting.RandomizeEnsureLegendary)
            {
                restrictedPokemon = args.staticPokemonSet.Where(PokemonUtils.IsLegendary);
                pokemonSettings = new Settings.PokemonSettings(settings.StaticEncounterSettings)
                {
                    BanLegendaries = false,
                };
            }
            else
            {
                restrictedPokemon = args.staticPokemonSet;
                pokemonSettings = settings.StaticEncounterSettings;
            }
            // Get new pokemon
            var newPokemon = pokeRand.RandomPokemonRestricted(args.pokemonSet, restrictedPokemon, pokemon, pokemonSettings);
            // Add to map if remap
            if (settings.RemapStaticEncounters)
            {
                args.staticPokemonMap.Add(pokemon, newPokemon);
            }
            // Remove from set if prevent dupes
            if (settings.PreventDuplicateStaticEncounters)
            {
                args.staticPokemonSet.Remove(newPokemon);
            }
            return newPokemon;
        }

        public class Args
        {
            public IEnumerable<ItemData> items;
            public IEnumerable<Pokemon> pokemonSet;
            // Gift Pokemon Randomization Params
            public HashSet<Pokemon> fossilSet;
            public HashSet<Pokemon> babySet;
            // Static Pokemon Randomization Params
            public HashSet<Pokemon> staticPokemonSet;
            public Dictionary<Pokemon, Pokemon> staticPokemonMap;
            public GymMetadata gymMetadata;
            public bool IsGym => gymMetadata != null;
        }
    }
}
