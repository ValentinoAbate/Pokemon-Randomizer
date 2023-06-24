using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI.Models
{
    public class PostgameAndSideContentDataModel : DataModel
    {
        public BattleFrontierDataModel FrontierData { get; set; } = new BattleFrontierDataModel();
        public BattleTentDataModel BattleTentData { get; set; } = new BattleTentDataModel();
    }
}
