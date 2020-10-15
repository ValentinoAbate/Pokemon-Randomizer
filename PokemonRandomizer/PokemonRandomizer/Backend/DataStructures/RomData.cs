using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    public class RomData
    {
        public const string gameCodeEm = "BPEE";
        public const string gameCodeLg = "BPGE";
        public const string gameCodeFr = "BPRE";
        public const string gameCodeRu = "AXVE";
        public const string gameCodeSp = "AXPE";
        public XmlManager Info { get; }
        private static readonly Dictionary<Generation, string> infoPaths = new Dictionary<Generation, string>
        {
            //{RomData.Generation.III, Path.Combine(Directory.GetCurrentDirectory(), "ROMInfo", "Gen3ROMInfo.xml") }
            {Generation.III, Resources.RomInfo.RomInfo.Gen3RomInfo }

        };
        public bool IsFireRedOrLeafGreen => Gen == Generation.III && (Code == gameCodeFr || Code == gameCodeLg);
        public bool IsRubySapphireOrEmerald => Gen == Generation.III && (Code == gameCodeEm || Code == gameCodeRu || Code == gameCodeSp);
        public enum Generation { I, II, III, IV, V, VI, VII }
        public Generation Gen { get; private set; }
        public string Code { get; private set; }
        public int Version { get; private set; }
        public string Name { get; private set; }

        // The original ROM the data was loaded from. Used by ROMWriter to write the data to a file.
        public Rom Rom { get; }
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
        public TypeEffectivenessChart TypeDefinitions { get; set; }
        public MapManager Maps { get; set; }
        public List<EncounterSet> Encounters { get; set; }
        public List<MoveData> MoveData { get; set; }

        #region TM, HM, and tutor move definition arrays
        public Move[] TMMoves;
        public Move[] HMMoves;
        public Move[] tutorMoves;
        #endregion

        #region Hacks and Tweaks

        public bool RunIndoors { get; set; } = false;
        public bool FastText { get; set; } = false;
        public bool UseUnknownTypeForMoves { get; set; } = false;
        public bool SnowyWeatherApplysHail { get; set; } = false;
        public bool EvolveWithoutNationalDex { get; set; } = true;

        #endregion

        public byte[] SkippedLearnSetData { get; set; }

        public RomData(byte[] rawRom)
        {
            InitGeneration(rawRom);
            Info = new XmlManager(infoPaths[Gen]);
            Info.SetSearchRoot("versionInfo");
            InitMetaData(rawRom);
            Info.SetSearchRoot(Code + Version.ToString());
            //Initalize Rom file wrapper
            var freeSpaceByte = (byte)Info.HexAttr("freeSpace", "byte");
            var searchStartOffset = Info.HexAttr("freeSpace", "startAddy");
            Rom = new Rom(rawRom, freeSpaceByte, searchStartOffset);
        }
        // set the Rom generation (from the file size)
        private void InitGeneration(byte[] rawRom)
        {
            switch (rawRom.Length)
            {
                case 1048576:    // 1mb
                    Gen = Generation.I;
                    break;
                case 2097152:    // 2mb
                    Gen = Generation.II;
                    break;
                case 16777216:   // 16mb
                    Gen = Generation.III;
                    break;
                case 67108864:   // 64mb  (diamond and pearl)
                case 134217728:  // 128mb (heart gold, soul silver, and platinum)
                    Gen = Generation.IV;
                    break;
                case 268435456:  // 256mb (black and white)
                case 536870912:  // 512mb (black 2 and white 2)
                    Gen = Generation.V;
                    break;
                default:
                    //Add fallback based on file extension?
                    //Add manual override?
                    throw new Exception("rom file is not a valid length, unable to detect generation");
            }
        }
        // read code, name and version info from rom
        private void InitMetaData(byte[] rawRom)
        {
            switch (Gen)
            {
                case Generation.I:
                    break;
                case Generation.II:
                    break;
                case Generation.III:
                    Name = Encoding.ASCII.GetString(rawRom.ReadBlock(Info.Offset("romName"), Info.Size("romName")));
                    Code = Encoding.ASCII.GetString(rawRom.ReadBlock(Info.Offset("code"), Info.Size("code")));
                    Version = rawRom[Info.Offset("version")];
                    break;
                case Generation.IV:
                    break;
                case Generation.V:
                    break;
                case Generation.VI:
                    break;
                case Generation.VII:
                    break;
                default:
                    throw new Exception("Gen " + Gen.ToDisplayString() + " is not supported. unable to find metadata");
            }
        }
        // returns a clone of this rom data
        public RomData Clone()
        {
            return Reading.RomParser.Parse(Rom.File);
        }
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