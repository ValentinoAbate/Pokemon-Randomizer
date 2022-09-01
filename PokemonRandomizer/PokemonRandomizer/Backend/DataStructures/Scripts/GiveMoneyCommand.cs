namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class GiveMoneyCommand : MoneyCommand
    {
        public override string ToString()
        {
            return $"Give {Amount} money to player ({Disable})";
        }
    }
}
