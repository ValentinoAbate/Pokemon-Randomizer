using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.UI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonRandomizer.UI.Views
{
    public class PostgameAndSideContentDataView : DataView<PostgameAndSideContentDataModel>
    {
        public PostgameAndSideContentDataView(PostgameAndSideContentDataModel model, RomMetadata metadata)
        {
            var tabs = CreateMainTabControl();

            if (metadata.IsEmerald)
            {
                tabs.Add(CreateTabItem("Battle Frontier", new BattleFrontierDataView(model.FrontierData)));
                tabs.Add(CreateTabItem("Battle Tents", new BattleTentDataView(model.BattleTentData)));
            }
        }
    }
}
