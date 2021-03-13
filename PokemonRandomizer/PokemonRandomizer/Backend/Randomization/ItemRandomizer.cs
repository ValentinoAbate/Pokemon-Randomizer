using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Randomization
{
    using EnumTypes;
    using DataStructures;
    public class ItemRandomizer
    {
        private readonly Random rand;
        private readonly Func<Item, ItemData> getData;
        public ItemRandomizer(Random rand, Func<Item, ItemData> getData)
        {
            this.rand = rand;
            this.getData = getData;
        }
        private ItemData GetData(Item item) => getData(item);

        public Item RandomItem(IEnumerable<ItemData> possibleItems, Item input, Settings settings)
        {
            // If the item is none, do special randomization
            if (input == Item.None)
            {
                return rand.RollSuccess(settings.NoneToOtherChance) ? rand.Choice(possibleItems).Item : input;
            }
            var inputData = GetData(input);
            if (inputData.IsKeyItem && settings.KeepKeyItems)
                return input;
            var itemWeights = new WeightedSet<ItemData>(possibleItems, 1);
            if (rand.RollSuccess(settings.SamePocketChance))
            {
                itemWeights.RemoveWhere((i) => i.pocket != inputData.pocket);
            }
            return itemWeights.Count <= 0 ? input : rand.Choice(itemWeights).Item;
        }

        public class Settings
        {
            public bool KeepKeyItems { get; set; } = true;
            public double SamePocketChance { get; set; } = 1;
            public double NoneToOtherChance { get; set; } = 0;
        }
    }
}
