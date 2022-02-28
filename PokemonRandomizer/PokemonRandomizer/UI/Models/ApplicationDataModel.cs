using static PokemonRandomizer.Settings;

namespace PokemonRandomizer.UI.Models
{
    public class ApplicationDataModel
    {
        public TmHmTutorModel TmHmTutorData { get; set; } = new TmHmTutorModel();
        public VariantPokemonDataModel VariantPokemonData { get; set; } = new VariantPokemonDataModel();
        public SpecialPokemonDataModel SpecialPokemonData { get; set; } = new SpecialPokemonDataModel();
        public PokemonTraitsModel PokemonData { get; set; } = new PokemonTraitsModel();
        public WildEncounterDataModel WildEncounterData { get; set; } = new WildEncounterDataModel();
        public TrainerDataModel TrainerData { get; set; } = new TrainerDataModel();
        public TrainerOrganizationDataModel TrainerOrgData { get; set; } = new TrainerOrganizationDataModel();
        public ItemDataModel ItemData { get; set; } = new ItemDataModel();
        public WeatherDataModel WeatherData { get; set; } = new WeatherDataModel();
        public MiscDataModel MiscData { get; set; } = new MiscDataModel();
    }
}
