namespace PokemonRandomizer.Backend.DataStructures
{
    using EnumTypes;
    public class InGameTrade
    {
        public const int pokemonNameLength = 12;
        public const int trainerNameLength = 11;
        public const byte maxIV = 31;
        public const byte maxIVPlus1 = maxIV + 1;

        public string pokemonName;
        public Pokemon pokemonRecieved;
        public byte[] IVs = new byte[6];
        public int abilityNum;
        public int trainerID;
        public byte[] contestStats = new byte[5];
        public int personality;
        public Item heldItem;
        public byte mailNum;
        public string trainerName;
        public byte trainerGender;
        public byte sheen;
        public Pokemon pokemonWanted;

        public override string ToString()
        {
            return "Trade " + pokemonWanted.ToString() + " for " + pokemonRecieved.ToString();
        }
    }
}
