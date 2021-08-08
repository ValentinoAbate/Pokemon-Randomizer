namespace PokemonRandomizer.UI
{
    using CompatOption = Settings.TmMtCompatOption;
    public class TmHmTutorModel : DataModel
    {
        public CompatOption TmCompatOption { get; set; } = CompatOption.Intelligent;
        public CompatOption TutorCompatOption { get; set; } = CompatOption.Intelligent;
        public CompatOption HmCompatOption { get; set; } = CompatOption.Unchanged;
        public double RandomCompatTrueChance { get; set; } = 0.42;
        public double IntelligentCompatNoise { get; set; } = 0.15;
        public bool NoHmMovesInTMsAndTutors { get; set; } = true;
        public bool NoDuplicateTMsAndTutors { get; set; } = true;
        public bool KeepImportantTmsAndTutors { get; set; } = true;

        public bool RandomizeTMs { get; set; } = false;
        public double TMRandChance { get; set; } = 1;
        public bool RandomizeMoveTutors { get; set; } = false;
        public double MoveTutorRandChance { get; set; } = 1;
    }
}
