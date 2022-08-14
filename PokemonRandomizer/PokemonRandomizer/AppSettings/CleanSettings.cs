using PokemonRandomizer.UI.Models;
using System;
using System.Collections.Generic;

namespace PokemonRandomizer.AppSettings
{
    public class CleanSettings : HardCodedSettings
    {
        public CleanSettings(ApplicationDataModel data) : base(data) { }

        public override bool WriteCatchingTutPokemon => false; // Method of writing works properly but causes known diffs

        public override HailHackOption HailHackSetting => HailHackOption.None;

        #region Misc

        // Gen II-IV Hacks and Tweaks
        public override bool UpdateDOTMoves => false;

        // Gen III Hacks and Tweaks
        public override bool RunIndoors => false;

        public override bool StartWithNationalDex => false;

        // FRLG Hacks and Tweaks
        public override bool EvolveWithoutNationalDex => false;

        // RSE Hacks and Tweaks
        public override bool EasyFirstRivalBattle => false;

        // FRLG + E Hacks and Tweaks
        public override bool DeoxysMewObeyFix => false;

        #endregion
    }
}
