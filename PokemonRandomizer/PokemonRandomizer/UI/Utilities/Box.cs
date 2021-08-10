namespace PokemonRandomizer.UI.Utilities
{
    public class Box<T>
    {
        public T Value { get; set; }

        public Box() : this(default) { }

        public Box(T val)
        {
            Value = val;
        }

        public static implicit operator T(Box<T> b) => b.Value;
    }
}
