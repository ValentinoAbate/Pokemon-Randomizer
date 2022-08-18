using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
    public interface IHasWeather
    {
        public Map.Weather Weather { get; set; }
    }
}
