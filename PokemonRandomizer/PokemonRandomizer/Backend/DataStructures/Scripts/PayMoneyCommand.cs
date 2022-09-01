namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class PayMoneyCommand : MoneyCommand
    {
        public override string ToString()
        {
            return $"Take {Amount} money from player ({Disable})";
        }
    }
}
