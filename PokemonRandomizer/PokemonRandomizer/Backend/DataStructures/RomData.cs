using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomData
    {


        // A metrics database calculated from the input ROM data (the base game if the rom being loaded is normal)
        public RomMetrics Metrics { get; private set; }
        public Item PcStartItem { get; set; }
        public List<PokemonSpecies> Starters { get; set; }
        public List<Item> StarterItems { get; set; }
        public PokemonSpecies CatchingTutPokemon { get; set; }
        public List<PokemonBaseStats> Pokemon { get; set; }
        public PokemonBaseStats[] PokemonDexOrder
        {
            get
            {
                PokemonBaseStats[] shallowClone = Pokemon.ToArray();
                Array.Sort(shallowClone, (x, y) => x.DexIndex.CompareTo(y.DexIndex));
                return shallowClone;
            }
        }
        public Dictionary<PokemonSpecies, PokemonBaseStats> PokemonLookup { get; set; }
        public List<string> ClassNames { get; set; }
        public List<Trainer> Trainers { get; set; }

        #region Special Trainer Info

        public Dictionary<string, Trainer> NormalTrainers { get; set; }
        public Dictionary<string, List<Trainer>> SpecialTrainers { get; set; }
        public List<Trainer> GruntBattles { get; set; }
        public string[] RivalNames { get; set; }
        public string[] GymLeaderNames { get; set; }
        public string[] EliteFourNames { get; set; }
        public string[] ChampionNames { get; set; }
        public string[] UberNames { get; set; }
        public string[] TeamGruntNames { get; set; }
        public string[] TeamAdminNames { get; set; }
        public string[] TeamLeaderNames { get; set; }
        public List<string> ReoccuringTrainerNames { get; set; }
        public List<string> AceTrainerNames { get; set; }
        public int[] AceTrainerClasses { get; set; }

        #endregion

        public TypeEffectivenessChart TypeDefinitions { get; set; }
        public IEnumerable<Map> Maps => MapBanks.SelectMany((bank) => bank);
        public Map[][] MapBanks { get; set; }
        public List<EncounterSet> Encounters { get; set; }
        public List<MoveData> MoveData { get; set; }

        #region TM, HM, and tutor move definition arrays
        public Move[] TMMoves;
        public Move[] HMMoves;
        public Move[] tutorMoves;
        #endregion

        public int[] RivalRemap { get; set; }

        #region Hacks and Tweaks

        public bool RunIndoors { get; set; } = false;
        public bool FastText { get; set; } = false;
        public bool UseUnknownTypeForMoves { get; set; } = false;
        public bool SnowyWeatherApplysHail { get; set; } = false;
        public bool EvolveWithoutNationalDex { get; set; } = true;

        #endregion

        public byte[] SkippedLearnSetData { get; set; }

        // updates the metrics from the current data
        public void CalculateMetrics()
        {
            Metrics = new RomMetrics(this);
        }
        /// <summary>updates the "evolvesFrom" fields of all pokemon</summary> 
        public void LinkEvolutions()
        {
            foreach (var pokemon in Pokemon)
                pokemon.evolvesFrom.Clear();
            foreach (var pokemon in Pokemon)
                foreach (var evo in pokemon.evolvesTo)
                    if (evo.Type != EvolutionType.None)
                        PokemonLookup[evo.Pokemon].evolvesFrom.Add(new Evolution(evo.Type, pokemon.species, evo.parameter));
        }
        /// <summary>
        /// updates the "eggMoves" fields of all pokemon from their "evolvesFrom" field 
        /// Link evolutions must be called before this function or it will not work properly
        /// </summary> 
        public void LinkEggMoves()
        {
            List<Move> GetEggMoves(PokemonBaseStats pokemon)
            {
                if (pokemon.eggMoves.Count > 0)
                    return pokemon.eggMoves;
                if (pokemon.evolvesFrom.Count > 0)
                    return GetEggMoves(PokemonLookup[pokemon.evolvesFrom[0].Pokemon]);
                return pokemon.eggMoves;
            }
            foreach (var pokemon in Pokemon)
            {
                if(pokemon.eggMoves.Count <= 0 && pokemon.evolvesFrom.Count > 0)
                {
                    pokemon.eggMoves = GetEggMoves(PokemonLookup[pokemon.evolvesFrom[0].Pokemon]);
                }
            }
        }

        // Create string array to write to info file
        public string[] ToStringArray()
        {
            const string divider = "=======================================================================" +
                                   "========================================================================";
            List<string> outLs = new List<string>();

            #region Print Pokemon Definitions
            outLs.Add(divider);
            outLs.Add("   Pokémon Info List");
            outLs.Add(divider);
            outLs.Add(("Pkmn Name      |  HP  AT  DF  SP  SA  SD   |    EV Yields   |" +
                " Type(s) |  Ability 1     | Ability 2     | Held Item 1   | Held Item 2   | "));
            outLs.Add(("---------------------------------------------------------" +
                      "----------------------------------------------------------------------------"));
            var pkmnSorted = PokemonDexOrder;
            foreach (PokemonBaseStats pkmn in pkmnSorted)
            {
                outLs.Add(pkmn.ToString());
                #region TM/HM/MT compat printing (Make prettier)
                //string compatStr = "TM Compat: ";
                //for (int i = 0; i < pkmn.TMCompat.Length; ++i)
                //{
                //    if (pkmn.TMCompat[i])
                //        compatStr += (i + 1) + ": " + TMMoves[i].ToDisplayString() + ", ";
                //}
                //compatStr += "||| HM Compat: ";
                //for (int i = 0; i < pkmn.HMCompat.Length; ++i)
                //{
                //    if (pkmn.HMCompat[i])
                //        compatStr += (i + 1) + ": " + HMMoves[i].ToDisplayString() + ", ";
                //}
                //compatStr += "||| Tutor Compat: ";
                //for (int i = 0; i < pkmn.moveTutorCompat.Length; ++i)
                //{
                //    if (pkmn.moveTutorCompat[i])
                //        compatStr += tutorMoves[i].ToDisplayString() + ", ";
                //}
                //outLs.Add(compatStr);
                #endregion
            }
            #endregion

            foreach(var move in MoveData)
            {
                outLs.Add(move.ToString() + " - effect: " + move.effect.ToString());
            }

            return outLs.ToArray();
        }
    }
}