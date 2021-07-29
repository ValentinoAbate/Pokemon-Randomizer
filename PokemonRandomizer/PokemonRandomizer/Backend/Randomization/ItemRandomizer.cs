﻿using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Randomization
{
    using DataStructures;
    using EnumTypes;
    public class ItemRandomizer
    {
        private readonly Random rand;
        private readonly IDataTranslator dataT;
        public ItemRandomizer(Random rand, IDataTranslator dataT)
        {
            this.rand = rand;
            this.dataT = dataT;
        }

        public Item RandomItem(IEnumerable<ItemData> possibleItems, Item input, Settings settings)
        {
            // If the item is none, do special randomization
            if (input == Item.None)
            {
                return rand.RollSuccess(settings.NoneToOtherChance) ? rand.Choice(possibleItems).Item : input;
            }
            var inputData = dataT.GetItemData(input);
            if (inputData.IsKeyItem && settings.DontRandomizeKeyItems)
                return input;
            var itemWeights = new WeightedSet<ItemData>(possibleItems);
            if (rand.RollSuccess(settings.SamePocketChance))
            {
                itemWeights.RemoveWhere(i => i.pocket != inputData.pocket);
            }
            if (settings.BanKeyItems)
            {
                itemWeights.RemoveWhere(i => i.IsKeyItem);
            }
            if (settings.BanMail)
            {
                itemWeights.RemoveWhere(i => ItemData.IsMail(i.Item));
            }
            return itemWeights.Count <= 0 ? input : rand.Choice(itemWeights).Item;
        }

        public class Settings
        {
            public bool DontRandomizeKeyItems { get; set; } = true;
            public bool BanKeyItems { get; set; } = true;
            public bool BanMail { get; set; } = true;
            public double SamePocketChance { get; set; } = 1;
            public double NoneToOtherChance { get; set; } = 0;
        }
    }
}
