using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Metadata;
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
                PokemonLookup.EnsureCapacity(value.Count);
                // Pre-process
                foreach (var p in pokemon)
                {
                    PokemonLookup.Add(p.species, p);
                    if(p.eggMoves == null)
                    {
                        p.eggMoves = new List<Move>(0);
                    }
                }
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
        private Dictionary<Pokemon, PokemonBaseStats> PokemonLookup { get; } = new(0);
        public List<string> ClassNames { get; set; }
        public List<Trainer> Trainers { get; set; }

        #region Special Trainer Info

        public Dictionary<string, PokemonType[]> TrainerNameTypeOverrides { get; } = new();
        public Dictionary<string, PokemonType[]> TrainerClassTypeOverrides { get; } = new();

        public List<VillainousTeamMetadata> VillainousTeamMetadata { get; } = new();

        #endregion

        public TypeEffectivenessChart TypeDefinitions { get; set; }
        public IEnumerable<Map> Maps => MapBanks.SelectMany(bank => bank);
        public Map[][] MapBanks { get; set; }
        public List<EncounterSet> Encounters { get; set; }
        // Used in dream team application
        public EncounterSet FirstEncounterSet { get; set; }
        private List<MoveData> moveData;
        public List<MoveData> MoveData 
        { 
            get => moveData;
            set
            {
                moveData = value;
                MoveDataLookup.Clear();
                MoveDataLookup.EnsureCapacity(value.Count);
                // Pre-process
                foreach (var m in moveData)
                {
                    MoveDataLookup.Add(m.move, m);
                }
            }
        }
        private Dictionary<Move, MoveData> MoveDataLookup { get; } = new(0);
        private List<ItemData> itemData;
        public List<ItemData> ItemData
        {
            get => itemData;
            set
            {
                itemData = value;
                ItemDataLookup.Clear();
                ItemDataLookup.EnsureCapacity(value.Count);
                // Pre-process
                foreach (var item in itemData)
                {
                    if(!item.IsUnused && !ItemDataLookup.ContainsKey(item.Item))
                    {
                        ItemDataLookup.Add(item.Item, item);
                    }
                }
            }
        }
        public List<ItemData> MysteryGiftEventItems { get; } = new(10);
        private Dictionary<Item, ItemData> ItemDataLookup { get; } = new(0);
        public PickupData PickupItems { get; set; }
        public List<InGameTrade> Trades { get; set; }

        #region TM, HM, and tutor move definition arrays
        public Move[] TMMoves;
        public Move[] HMMoves;
        public Move[] tutorMoves;
        #endregion

        public Script SetBerryTreeScript { get; set; }

        public int[] RivalRemap { get; set; }

        public byte[] SkippedLearnSetData { get; set; }

        public List<Item> NewEvolutionStones { get; set; } = new List<Item>();

        public string PaletteOverrideKey { get; set; }

        // Container for data to be written to the info file that cannot be inferred from the general rom data
        public Dictionary<string, List<string>> RandomizationResults { get; } = new Dictionary<string, List<string>>();

        public PokemonBaseStats GetBaseStats(Pokemon p) => PokemonLookup[p];
        public MoveData GetMoveData(Move m) => MoveDataLookup[m];
        public ItemData GetItemData(Item i) => ItemDataLookup[i];
        public Trainer GetTrainer(int trainerIndex) => Trainers[trainerIndex];
        public HashSet<Move> GetAllMoves()
        {
            var set = new HashSet<Move>(MoveDataLookup.Count);
            foreach(var kvp in MoveDataLookup)
            {
                set.Add(kvp.Key);
            }
            return set;
        }
        public List<ItemData> GetAllValidItemData()
        {
            var list = new List<ItemData>(ItemData.Count);
            foreach(var item in ItemData)
            {
                if (item.Item == Item.None || item.IsUnused)
                    continue;
                list.Add(item);
            }
            return list;
        }
        public HashSet<Item> GetAllItems()
        {
            var set = new HashSet<Item>(ItemDataLookup.Count);
            foreach (var kvp in ItemDataLookup)
            {
                set.Add(kvp.Key);
            }
            return set;
        }
        public List<Item> GetAllItemsOrdered(bool includeNone = false)
        {
            var list = new List<Item>(includeNone ? ItemData.Count + 1 : ItemData.Count);
            if (includeNone)
            {
                list.Add(Item.None);
            }
            foreach (var item in ItemData)
            {
                if (item.IsUnused)
                    continue;
                list.Add(item.Item);
            }
            return list;
        }

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
                        PokemonLookup[evo.Pokemon].evolvesFrom.Add(new Evolution(evo) { Pokemon = pokemon.species });
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