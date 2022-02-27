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
        public static byte MapToByte(double max, double value)
        {
            //compute t
            double t = (1 / max) * value;
            return (byte)System.Math.Round(byte.MaxValue * t);
        }
    }
}
