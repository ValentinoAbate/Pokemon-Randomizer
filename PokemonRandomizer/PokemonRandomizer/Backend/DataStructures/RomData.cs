using PokemonRandomizer.Backend.EnumTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomData : IDataTranslator
    {


        // A metrics database calculated from the input ROM data (the base game if the rom being loaded is normal)
        public RomMetrics Metrics { get; private set; }
        public string Seed { get; set; }
        public Item PcStartItem { get; set; }
        public List<Pokemon> Starters { get; set; }
        public List<Item> StarterItems { get; set; }
        public Pokemon CatchingTutPokemon { get; set; }
        private List<PokemonBaseStats> pokemon;
        public List<PokemonBaseStats> Pokemon
        {
            get => pokemon;
            set
            {
                pokemon = value;
                // Refresh the lookup
                PokemonLookup.Clear();
                foreach (var p in pokemon)
                    PokemonLookup.Add(p.species, p);
                // Refresh the national dex order
                PokemonNationalDexOrder = Pokemon.ToArray();
                Array.Sort(PokemonNationalDexOrder, (x, y) => x.NationalDexIndex.CompareTo(y.NationalDexIndex));
                // Link the evolutions and egg moves
                LinkEvolutions();
                LinkEggMoves();
                LinkOriginalMoveLearns();
                // Set any necessary original values
                Pokemon.ForEach(p => p.SetOriginalValues());
                // Set up the names
                PokemonNames = PokemonNationalDexOrder.Select(p => p.Name).ToArray();
            }
        }
        public string[] PokemonNames { get; private set; }
        public PokemonBaseStats[] PokemonNationalDexOrder { get; private set; }
        private Dictionary<Pokemon, PokemonBaseStats> PokemonLookup { get; } = new Dictionary<Pokemon, PokemonBaseStats>();
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
        public IEnumerable<Map> Maps => MapBanks.SelectMany(bank => bank);
        public Map[][] MapBanks { get; set; }
        public List<EncounterSet> Encounters { get; set; }
        // Used in dream team application
        public EncounterSet FirstEncounterSet { get; set; }
        public List<MoveData> MoveData { get; set; }
        public List<ItemData> ItemData { get; set; }
        public PickupData PickupItems { get; set; }
        public List<InGameTrade> Trades { get; set; }

        #region TM, HM, and tutor move definition arrays
        public Move[] TMMoves;
        public Move[] HMMoves;
        public Move[] tutorMoves;
        #endregion

        public int[] RivalRemap { get; set; }

        public byte[] SkippedLearnSetData { get; set; }

        public List<Item> NewEvolutionStones { get; set; } = new List<Item>();

        public string PaletteOverrideKey { get; set; }

        public PokemonBaseStats GetBaseStats(Pokemon p) => PokemonLookup[p];
        public MoveData GetMoveData(Move m) => MoveData[(int)m];
        public ItemData GetItemData(Item i) => ItemData[(int)i];

        // updates the metrics from the current data
        public void CalculateMetrics()
        {
            Metrics = new RomMetrics(this);
        }
        /// <summary>updates the "evolvesFrom" fields of all pokemon</summary> 
        private void LinkEvolutions()
        {
            foreach (var pokemon in Pokemon)
                pokemon.evolvesFrom.Clear();
            foreach (var pokemon in Pokemon)
            {
                foreach (var evo in pokemon.evolvesTo)
                {
                    if (evo.Type != EvolutionType.None)
                    {
                        PokemonLookup[evo.Pokemon].evolvesFrom.Add(new Evolution(evo.Type, pokemon.species, evo.parameter));
                    }
                }
            }
        }
        /// <summary>
        /// updates the "eggMoves" fields of all pokemon from their "evolvesFrom" field 
        /// Link evolutions must be called before this function or it will not work properly
        /// </summary> 
        private void LinkEggMoves()
        {
            List<Move> GetEggMoves(PokemonBaseStats pokemon)
            {
                if (pokemon.eggMoves.Count > 0)
                    return pokemon.eggMoves;
                if (pokemon.evolvesFrom.Count > 0)
                    return GetEggMoves(GetBaseStats(pokemon.evolvesFrom[0].Pokemon));
                return pokemon.eggMoves;
            }
            foreach (var pokemon in Pokemon)
            {
                if(pokemon.eggMoves.Count <= 0 && pokemon.evolvesFrom.Count > 0)
                {
                    pokemon.eggMoves = GetEggMoves(GetBaseStats(pokemon.evolvesFrom[0].Pokemon));
                }
            }
        }

        private void LinkOriginalMoveLearns()
        {
            static void LinkMoves(BitArray arr, Move[] moves, HashSet<Move> moveOutputCanLearn, HashSet<Move> moveOutputCantLearn)
            {
                for (int i = 0; i < arr.Count; ++i)
                {
                    if (arr[i])
                    {
                        moveOutputCanLearn.Add(moves[i]);
                    }
                    else
                    {
                        moveOutputCantLearn.Add(moves[i]);
                    }
                }
            }
            foreach(var pokemon in Pokemon)
            { 
                pokemon.originalTmHmMtMoves.Clear();
                LinkMoves(pokemon.TMCompat, TMMoves, pokemon.originalTmHmMtMoves, pokemon.originalUnlearnableTmHmMtMoves);
                LinkMoves(pokemon.HMCompat, HMMoves, pokemon.originalTmHmMtMoves, pokemon.originalUnlearnableTmHmMtMoves);
                LinkMoves(pokemon.moveTutorCompat, tutorMoves, pokemon.originalTmHmMtMoves, pokemon.originalUnlearnableTmHmMtMoves);
            }
        }
    }
}