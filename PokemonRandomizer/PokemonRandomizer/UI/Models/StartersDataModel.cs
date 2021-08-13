namespace PokemonRandomizer.UI
{
    using Utilities;
    using static Settings;
    public class StartersDataModel : DataModel
    {
        public Box<StarterPokemonOption> StarterSetting { get; } = new Box<StarterPokemonOption>(StarterPokemonOption.Unchanged);
        public Box<bool> StrongStarterTypeTriangle { get; } = new Box<bool>();
        public string[] CustomStarters { get; } = new string[]
        {
            "Random",
            "Random",
            "Random",
        };
        public Box<bool> BanLegendaries { get; } = new Box<bool>(true);

        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public Box<bool> SafeStarterMovesets { get; } = new Box<bool>(true);
    }
}
