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
        // Later have preset ones for special groups
        public TrainerDataModel[] TrainerDataModels { get; set; } = new TrainerDataModel[]
        {
            new TrainerDataModel(TrainerCategory.Trainer, DataModel.defaultName),
            new TrainerDataModel(TrainerCategory.AceTrainer, "Ace Trainers"),
            new TrainerDataModel(TrainerCategory.Rival, "Rivals"),
            new TrainerDataModel(TrainerCategory.GymLeader, "Gym Leaders"),
            new TrainerDataModel(TrainerCategory.EliteFour, "Elite Four"),
            new TrainerDataModel(TrainerCategory.Champion, "Champion"),
        };
        public ItemDataModel ItemData { get; set; } = new ItemDataModel();
        public WeatherDataModel WeatherData { get; set; } = new WeatherDataModel();
        public MiscDataModel MiscData { get; set; } = new MiscDataModel();
    }
}
