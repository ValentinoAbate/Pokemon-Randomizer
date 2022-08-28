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
    }
}
