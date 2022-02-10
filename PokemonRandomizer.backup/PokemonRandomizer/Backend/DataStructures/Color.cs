using System;

namespace PokemonRandomizer.Backend.DataStructures
{
    public struct Color : IEquatable<Color>
    {
        public static Color Lerp(Color c1, Color c2, float lerpFactor)
        {
            return (c1 * (1 - lerpFactor)) + (c2 * lerpFactor);
        }

        public int Value => Math.Max(Math.Max(r, g), b);
        public int Hue
        {
            get
            {
                int max = Math.Max(Math.Max(r, g), b);
                int min = Math.Min(Math.Min(r, g), b);

                if (min == max)
                {
                    return 0;
                }

                float hue;
                if (max == r)
                {
                    hue = (g - b) / (float)(max - min);

                }
                else if (max == g)
                {
                    hue = 2f + (b - r) / (float)(max - min);

                }
                else // max == b
                {
                    hue = 4f + (r - g) / (float)(max - min);
                }

                hue *= 60;
                if (hue < 0)
                {
                    hue += 360;
                }

                return (int)Math.Round(hue);
            }
        }
        public int r;
        public int g;
        public int b;
        public int a;

        public Color(int color, int alpha = 0)
        {
            r = g = b = color;
            a = alpha;
        }

        public Color(int r, int g, int b, int a)
        {
            this.r = Math.Max(r, 0);
            this.g = Math.Max(g, 0);
            this.b = Math.Max(b, 0);
            this.a = a;
        }

        public void Clamp(int min, int max)
        {
            r = Math.Max(Math.Min(r, max), min);
            g = Math.Max(Math.Min(g, max), min);
            b = Math.Max(Math.Min(b, max), min);
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

        public static Color operator+(Color left, Color right)
        {
            return new Color(left.r + right.r, left.g + right.g, left.b + right.b, left.a + right.b);
        }

        public static Color operator *(Color left, float right)
        {
            return new Color((int)Math.Round(left.r * right), (int)Math.Round(left.g * right), (int)Math.Round(left.b * right), left.a);
        }
    }
}
