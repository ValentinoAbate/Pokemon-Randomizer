namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using Backend.EnumTypes;
    using static Settings;
    public class StartersDataModel : DataModel
    {
        public Box<StarterPokemonOption> StarterSetting { get; set; } = new Box<StarterPokemonOption>(StarterPokemonOption.Unchanged);
        public Box<bool> StrongStarterTypeTriangle { get; set; } = new Box<bool>();
        public Pokemon[] CustomStarters { get; set; } = new Pokemon[]
        {
            Pokemon.None,
            Pokemon.None,
            Pokemon.None,
        };
        public Box<bool> BanLegendaries { get; set; } = new Box<bool>(true);

        /// <summary>
        /// Ensures that all starters have an attacking move at lvl1
        /// Currently just makes all starters additionally have tackle
        /// </summary>
        public Box<bool> SafeStarterMovesets { get; set; } = new Box<bool>(true);
    }
}
