namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class CheckMoneyCommand : MoneyCommand
    {
        public override string ToString()
        {
            return $"Check if player has at least {Amount} money ({Disable})";
        }
    }
}
