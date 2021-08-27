namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    using CompatOption = Settings.MoveCompatOption;
    public class TmHmTutorModel : DataModel
    {
        public Box<CompatOption> TmCompatOption { get; set; } = new Box<CompatOption>(CompatOption.Intelligent);
        public Box<CompatOption> TutorCompatOption { get; set; } = new Box<CompatOption>(CompatOption.Intelligent);
        public Box<CompatOption> HmCompatOption { get; set; } = new Box<CompatOption>(CompatOption.Unchanged);
        public Box<double> RandomCompatTrueChance { get; set; } = new Box<double>(0.42);
        public Box<double> IntelligentCompatNoise { get; set; } = new Box<double>(0.15);
        public Box<bool> NoHmMovesInTMsAndTutors { get; set; } = new Box<bool>(true);
        public Box<bool> NoDuplicateTMsAndTutors { get; set; } = new Box<bool>(true);
        public Box<bool> KeepImportantTmsAndTutors { get; set; } = new Box<bool>(true);

        public Box<bool> RandomizeTMs { get; set; } = new Box<bool>(false);
        public Box<double> TMRandChance { get; set; } = new Box<double>(1);
        public Box<bool> RandomizeMoveTutors { get; set; } = new Box<bool>(false);
        public Box<double> MoveTutorRandChance { get; set; } = new Box<double>(1);
    }
}
