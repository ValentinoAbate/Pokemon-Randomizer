namespace PokemonRandomizer.UI.Models
{
    using Utilities;
    public class MiscDataModel : DataModel
    {
        public Box<bool> UpdateDOTMoves { get; set; } = new Box<bool>(false);
        public Box<bool> RunIndoors { get; set; } = new Box<bool>(true);
        public Box<bool> EnableEvents { get; set; } = new Box<bool>(true);
        public Box<Settings.MysteryGiftItemSetting> EventItemSetting { get; set; } = new Box<Settings.MysteryGiftItemSetting>(Settings.MysteryGiftItemSetting.AllowInRandomization);
        public Box<bool> CountRelicanthAsFossil { get; set; } = new Box<bool>(true);

        // FRLG Only
        public Box<bool> EvolveWithoutNationalDex { get; set; } = new Box<bool>(true);

        // RSE Only
        public Box<bool> RandomizeWallyAce { get; set; } = new Box<bool>(true);
        public Box<bool> EasyFirstRivalbattle { get; set; } = new Box<bool>(true);

        public Box<bool> RandomizeBerryTrees { get; set; } = new Box<bool>(false);
        public Box<double> BerryTreeRandomizationChance { get; set; } = new Box<double>(1);
        public Box<bool> BanEvBerries { get; set; } = new Box<bool>(false);
        public Box<bool> BanMinigameBerries { get; set; } = new Box<bool>(true);
        public Box<bool> RemapBerries { get; set; } = new Box<bool>(true);

        // FRLG + E Only
        public Box<bool> DeoxysMewObeyFix { get; set; } = new Box<bool>(true);
    }
}
