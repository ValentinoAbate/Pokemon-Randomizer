using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI
{
    public class UISkin
    {
        public static UISkin Current { get; set; } = new UISkin();


        //Text

        public string HacksAndTweaksHeader { get; set; } = "Hacks and Tweaks";
    }
}
