using System;
using System.Collections.Generic;
using System.Linq;
using static PokemonRandomizer.Backend.DataStructures.ItemData;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    using PokemonRandomizer.Backend.Utilities;
    using PokemonRandomizer.Backend.Utilities.Debug;

    public class ItemRandomizer
    {
        private readonly Random rand;
        private readonly RandomizerSettings randomizerSettings;
        private readonly IDataTranslator dataT;
        private readonly Dictionary<Item, int> occurences = new Dictionary<Item, int>();
        public ItemRandomizer(Random rand, RandomizerSettings settings, IDataTranslator dataT)
        {
            this.rand = rand;
            this.dataT = dataT;
            randomizerSettings = settings;
        }

        public Item RandomItem(IEnumerable<ItemData> possibleItems, Item input, Settings settings)
        {
            var itemWeights = new WeightedSet<ItemData>(possibleItems.Where(IsNotBanned));
            // Apply antiduplicate weights (if necessary)
            if (randomizerSettings.OccurenceWeightMultiplier > 0)
            {
                itemWeights.Multiply(OccurenceWeight);
            }
            // If the item is none, do special randomization
            if (input == Item.None)
            {
                if (rand.RollSuccess(settings.NoneToOtherChance))
                {
                    var noneToOtherChoice = rand.Choice(itemWeights).Item;
                    UpdateOccurences(noneToOtherChoice);
                    return noneToOtherChoice;
                }
                return input;
            }
            var inputData = dataT.GetItemData(input);
            if (IsSkipped(inputData))
            {
                return input;
            }
            var validCategories = randomizerSettings.SameCategoryCategories & inputData.ItemCategories;
            // If validCategories == Categories.None, then any category is valid
            if (validCategories != Categories.None && rand.RollSuccess(randomizerSettings.SameCategoryChance))
            {
                itemWeights.RemoveWhere(i => !HasAnyCategory(i.ItemCategories, validCategories));
            }
            if (itemWeights.Count <= 0)
            {
                return input;
            }
            var choice = rand.Choice(itemWeights).Item;
            UpdateOccurences(choice);
            return choice;
        }

        private void UpdateOccurences(Item i)
        {
            if (!occurences.ContainsKey(i))
            {
                occurences.Add(i, 1);
                return;
            }
            occurences[i] = occurences[i] + 1;
        }

        private float OccurenceWeight(ItemData i)
        {
            if (!HasAnyCategory(i.ItemCategories, randomizerSettings.OccurenceWeightedCategories) || !occurences.ContainsKey(i.Item))
            {
                return 1;
            }
            return 1 / (occurences[i.Item] * (float)randomizerSettings.OccurenceWeightMultiplier);
        }

        private bool IsNotBanned(ItemData i)
        {
            return i.ItemCategories == Categories.None || ((randomizerSettings.BannedCategories & i.ItemCategories) == Categories.None);
        }

        private bool IsSkipped(ItemData i) => HasAnyCategory(randomizerSettings.SkipCategories, i.ItemCategories);

        private bool HasAnyCategory(Categories c1, Categories c2)
        {
            return (c1 & c2) != Categories.None;
        }

        public void LogOccurrences()
        {
            var list = new List<KeyValuePair<Item, int>>(occurences);
            list.Sort((i1, i2) => i1.Key.CompareTo(i2.Key));
            foreach(var i in list)
            {
                Logger.main.Info($"{i.Key.ToDisplayString()}: {i.Value}");
            }
        }

        public class RandomizerSettings
        {
            // Items in these categories will not be randomized
            public Categories SkipCategories { get; set; } = Categories.KeyItem | Categories.ContestScarf;
            // Items in these categories will not be selected from the random pool
            public Categories BannedCategories { get; set; } = Categories.KeyItem | Categories.ContestScarf | Categories.Mail | Categories.MinigameBerry;
            // Items in these categories will be less likely to be chosen if they have been chosed before
            public Categories OccurenceWeightedCategories { get; set; }// = Categories.TM | Categories.HeldItem;
            public double OccurenceWeightMultiplier { get; set; } = 10;
            // Items in this categories will be more like to be replaced with an item from the same category when replaced
            public Categories SameCategoryCategories { get; set; }
            public double SameCategoryChance { get; set; } = 0.75;
        }

        public class Settings
        {
            public bool BanMail { get; set; } = true;
            public double SamePocketChance { get; set; } = 1;
            public double NoneToOtherChance { get; set; } = 0;
        }
    }
}
