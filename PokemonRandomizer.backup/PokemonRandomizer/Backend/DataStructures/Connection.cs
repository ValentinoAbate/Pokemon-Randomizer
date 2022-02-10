﻿namespace PokemonRandomizer.Backend.DataStructures
{
    public class Connection
    {
        public enum Type
        {
            Down = 1,
            Up,
            Left,
            Right,
            Dive,
            Emerge,
        }
        public Type type;
        public int offset;
        public byte bankId;
        public byte mapId;
        public int unknown;
    }
}
