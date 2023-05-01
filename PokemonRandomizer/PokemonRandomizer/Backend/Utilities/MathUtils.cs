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

        public static int MapInt(int currentMax, int newMax, int value)
        {
            return (int)System.Math.Round(Map(currentMax, newMax, value));
        }

        public static double Map(double currentMax, double newMax, double value)
        {
            // compute t (assumes current min and new min are 0)
            double t = (1 / currentMax) * value;
            return newMax * t;
        }

        /// <summary> Get the number of base-10 digits in an int32 (for display purposes). The negative sign is considered a digit </summary>
        public static int NumDigits(int number)
        {
            if (number >= 0)
            {
                if (number < 10) return 1;
                if (number < 100) return 2;
                if (number < 1000) return 3;
                if (number < 10000) return 4;
                if (number < 100000) return 5;
                if (number < 1000000) return 6;
                if (number < 10000000) return 7;
                if (number < 100000000) return 8;
                if (number < 1000000000) return 9;
                return 10;
            }
            else
            {
                if (number > -10) return 2;
                if (number > -100) return 3;
                if (number > -1000) return 4;
                if (number > -10000) return 5;
                if (number > -100000) return 6;
                if (number > -1000000) return 7;
                if (number > -10000000) return 8;
                if (number > -100000000) return 9;
                if (number > -1000000000) return 10;
                return 11;
            }
        }

        /// <summary> Get the number of base-10 digits in a byte (for display purposes) </summary>
        public static int NumDigits(byte number)
        {
            if (number < 10) return 1;
            if (number < 100) return 2;
            return 3;
        }
    }
}
