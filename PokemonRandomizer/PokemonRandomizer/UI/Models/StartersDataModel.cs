namespace PokemonRandomizer.UI
{
    public class StartersDataModel : DataModel
    {
        public Settings.StarterPokemonOption StarterSetting { get; set; } = Settings.StarterPokemonOption.Unchanged;
        public bool StrongStarterTypeTriangle { get; set; } = false;
        public string[] CustomStarters { get; } = new string[]
        {
            "Random",
            "Random",
            "Random",
        };
        public bool BanLegendaries { get; set; } = true;

        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public bool SafeStarterMovesets { get; set; } = true;
    }
}
