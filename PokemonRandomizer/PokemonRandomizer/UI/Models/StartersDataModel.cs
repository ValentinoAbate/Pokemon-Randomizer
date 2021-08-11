namespace PokemonRandomizer.UI
{
    using Utilities;
    public class StartersDataModel : DataModel
    {
        public Settings.StarterPokemonOption StarterSetting { get; set; } = Settings.StarterPokemonOption.Unchanged;
        public Box<bool> StrongStarterTypeTriangle { get; set; } = new Box<bool>();
        public string[] CustomStarters { get; } = new string[]
        {
            "Random",
            "Random",
            "Random",
        };
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);

        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public Box<bool> SafeStarterMovesets { get; set; } = new Box<bool>(true);
    }
}
