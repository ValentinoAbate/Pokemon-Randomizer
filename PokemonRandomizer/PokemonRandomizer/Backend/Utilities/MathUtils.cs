using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    public class MathUtils
    {
        public static int IntPow(int @base, int exp)
        {
            int result = 1;
            for (int i = 0; i < exp; i++)
                result *= @base;
            return result;
        }
    }
}
