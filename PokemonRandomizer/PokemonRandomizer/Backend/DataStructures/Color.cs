using System;

namespace PokemonRandomizer.Backend.DataStructures
{
    public struct Color : IEquatable<Color>
    {
        public ushort r;
        public ushort g;
        public ushort b;
        public ushort a;

        public Color(ushort r, ushort g, ushort b, ushort a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(int r, int g, int b, int a)
        {
            this.r = (ushort)r;
            this.g = (ushort)g;
            this.b = (ushort)b;
            this.a = (ushort)a;
        }

        public override bool Equals(object obj)
        {
            return obj is Color color && Equals(color);
        }

        public bool Equals(Color other)
        {
            return r == other.r &&
                   g == other.g &&
                   b == other.b &&
                   a == other.a;
        }

        public override int GetHashCode()
        {
            int hashCode = -490236692;
            hashCode = hashCode * -1521134295 + r.GetHashCode();
            hashCode = hashCode * -1521134295 + g.GetHashCode();
            hashCode = hashCode * -1521134295 + b.GetHashCode();
            hashCode = hashCode * -1521134295 + a.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"r:{r} g:{g} b:{b} a:{a}";
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }
    }
}
