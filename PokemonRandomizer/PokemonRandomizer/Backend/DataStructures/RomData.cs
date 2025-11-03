using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.DataStructures.Trainers;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Metadata;
using PokemonRandomizer.Backend.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomData : IDataTranslator
    {
        private static int ComparePokemonByNationalDexOrder(PokemonBaseStats p1, PokemonBaseStats p2)
        {
            return p1.NationalDexIndex.CompareTo(p2.NationalDexIndex);
        }
        // A metrics database calculated from the input ROM data (the base game if the rom being loaded is normal)
        public RomMetrics Metrics { get; private set; }
        public string Seed { get; set; }
        public string VariantSeed { get; set; }
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
                Array.Sort(PokemonNationalDexOrder, ComparePokemonByNationalDexOrder);
                // Link the evolutions and egg moves
                LinkEvolutions();
                LinkEggMoves();
                LinkOriginalMoveLearns();

                // Post-process
                PokemonNames = new string[PokemonNationalDexOrder.Length];
                for(int i = 0; i < PokemonNationalDexOrder.Length; i++)
                {
                    var p = PokemonNationalDexOrder[i];
                    // Log name
                    PokemonNames[i] = p.Name;
                    // Set any necessary original values
                    p.SetOriginalValues();
                }
            }
        }
        public string[] PokemonNames { get; private set; }
        public PokemonBaseStats[] PokemonNationalDexOrder { get; private set; }
        private Dictionary<Pokemon, PokemonBaseStats> PokemonLookup { get; } = new(0);
        public List<TrainerClass> TrainerClasses { get; set; }
        public List<TrainerSprite> TrainerSprites { get; set; }
        public IEnumerable<Trainer> AllTrainers
        {
            get
            {
                foreach(var trainer in Trainers)
                {
                    yield return trainer;
                }
                foreach (var kvp in SpecialTrainers)
                {
                    yield return kvp.Value;
                }
            }
        }
        public List<BasicTrainer> Trainers { get; set; }
        public Dictionary<string, List<Trainer>> SpecialTrainers { get; } = new();

        #region Special Trainer Info

        public Dictionary<string, PokemonType[]> TrainerNameTypeOverrides { get; } = new();
        public Dictionary<string, PokemonType[]> TrainerClassTypeOverrides { get; } = new();

        public List<VillainousTeamMetadata> VillainousTeamMetadata { get; } = new();

        #endregion

        public TypeEffectivenessChart TypeDefinitions { get; set; }
        public IEnumerable<Map> Maps
        {
            get
            {
                foreach(var bank in MapBanks)
                {
                    foreach(var map in bank)
                    {
                        yield return map;
                    }
                }
            }
        }
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
        public Move[] TutorMoves 
        {
            get => tutorMoves;
            set
            {
                tutorMoves = value;
                OriginalTutorMoves = new Move[value.Length];
                Array.Copy(value, OriginalTutorMoves, value.Length);
            }
        }
        private Move[] tutorMoves;
        public Move[] OriginalTutorMoves { get; private set; }
        #endregion

        public Script SetBerryTreeScript { get; set; }
        public Dictionary<string, Script> SpecialScripts { get; } = new();

        // Battle Frontier and Minigames

        public List<FrontierTrainerPokemon> BattleFrontierTrainerPokemon { get; } = new();
        public List<FrontierBrainTrainerPokemon> BattleFrontierBrainPokemon { get; } = new();
        public List<Item> BattleFrontierHeldItems { get; } = new();
        public List<List<int>> BattleFrontierTutorIndices { get; } = new();
        public List<BattleTent> BattleTents { get; } = new(3);
        public byte[] RouletteWagers { get; set; } = Array.Empty<byte>();

        public int[] RivalRemap { get; set; }

        public List<Item> NewEvolutionStones { get; set; } = new List<Item>();

        public string PaletteOverrideKey { get; set; }

        // Container for data to be written to the info file that cannot be inferred from the general rom data
        public Dictionary<string, List<string>> RandomizationResults { get; } = new Dictionary<string, List<string>>();

        public List<PokemonType> Types { get; } = new(20);

        public PokemonBaseStats GetBaseStats(Pokemon p) => PokemonLookup[p];
        public MoveData GetMoveData(Move m) => MoveDataLookup[m];
        public Move GetTmMove(int index) => index >= 0 && index < TMMoves.Length ? TMMoves[index] : Move.None;
        public Move GetHmMove(int index) => index >= 0 && index < HMMoves.Length ? HMMoves[index] : Move.None;
        public Move GetTutorMove(int index) => index >= 0 && index < TutorMoves.Length ? TutorMoves[index] : Move.None;
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
        public HashSet<Move> GetValidMoves(bool ignoreHms, bool ignoreSelfdestruct)
        {
            var set = new HashSet<Move>(MoveDataLookup.Count);
            foreach (var kvp in MoveDataLookup)
            {
                if (ignoreSelfdestruct && kvp.Value.IsSelfdestruct)
                    continue;
                set.Add(kvp.Key);
            }
            set.RemoveIfContains(Move.None);
            set.RemoveIfContains(Move.STRUGGLE);
            if (ignoreHms)
            {
                foreach (var move in HMMoves)
                {
                    set.RemoveIfContains(move);
                }
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
                LinkMoves(pokemon.moveTutorCompat, TutorMoves, pokemon.originalTmHmMtMoves, pokemon.originalUnlearnableTmHmMtMoves);
            }
        }
    }
}