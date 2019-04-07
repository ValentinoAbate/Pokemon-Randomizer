using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class RomData
    {
        public XmlManager Info { get; }
        private static readonly Dictionary<Generation, string> infoPaths = new Dictionary<Generation, string>
        {
            //{RomData.Generation.III, Path.Combine(Directory.GetCurrentDirectory(), "ROMInfo", "Gen3ROMInfo.xml") }
            // Temporary debug path
            {RomData.Generation.III, "C:\\Users\\valen\\Documents\\GitHub\\Pokemon-Emerald-Randomizer\\PokemonEmeraldRandomizer\\PokemonEmeraldRandomizer\\RomInfo\\Gen3RomInfo.xml" }

        };
        public enum Generation { I,II,III,IV,V,VI,VII }
        public enum GameCode { AXVE, AXPE, BPEE, BPRE, BPGE }
        public Generation Gen { get; private set; }
        public string Code { get; private set; }
        public int Version { get; private set; }
        public string Name { get; private set; }

        // The original ROM the data was loaded from. Used by ROMWriter to write the data to a file.
        public Rom Rom { get; }
        // A metrics database calculated from the input ROM data (the base game if the rom being loaded is normal)
        public RomMetrics Metrics { get; private set; }
        public List<Pokemon> Starters { get; set; }
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
        public Dictionary<PokemonSpecies,PokemonBaseStats> PokemonLookup { get; set; }
        public List<string> ClassNames { get; set; }
        public List<Trainer> Trainers { get; set; }
        public TypeEffectivenessChart TypeDefinitions { get; set; }
        public MapManager Maps { get; set; }
        public List<EncounterSet> Encounters { get; set; }
        public List<MoveData> moveData { get; set; }

        #region TM, HM, and tutor move definition arrays
        public Move[] TMMoves;
        public Move[] HMMoves;
        public Move[] tutorMoves;
        #endregion

        public bool NationalDexUnlocked = false;

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
            return RomParser.Parse(Rom.File);
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
            foreach(var pokemon in Pokemon)
                foreach (var evo in pokemon.evolvesTo)
                    if(evo.Type != EvolutionType.None)
                        PokemonLookup[evo.Pokemon].evolvesFrom.Add(new Evolution(evo.Type, pokemon.species, evo.parameter));
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

            #region TODO
            //int[] TMlist = new int[8];

            //actualOrder = new String[411];
            //for (int i = 0; i < 411; i++, ptr += 8)
            //{
            //    sb = new StringBuilder("");
            //    if (i >= 251 && i < 276) continue;
            //    for (int p = 0; p < 8; p++)
            //    {      //get TM list
            //        TMlist[p] = rom[ptr + p];
            //    }
            //    sb.append(String.format("%-10s", arPkmn[i]));
            //    sb.append(" |");

            //    int TMptr = 1;
            //    for (int entry : TMlist)
            //    {   //print data
            //        for (int j = 1; j < 256; j *= 2, TMptr++)
            //        {
            //            if (TMptr >= 58) break;
            //            if ((entry & j) > 0)
            //            {
            //                sb.append(" X");
            //            }
            //            else
            //            {
            //                sb.append(" -");
            //            }
            //            if (TMptr % 5 == 0)
            //            {
            //                sb.append("|");
            //            }
            //        }
            //    }
            //    sb.append("\n");
            //    actualOrder[i] = sb.toString();
            //}
            //for (int i = 0; i < order.length; i++)
            //{
            //    if (i % 50 == 0)
            //    {            //print header every 50 entries
            //        outLs.Add("\n" + header);
            //    }
            //    outLs.Add(String.format("%03d.", i + 1) + actualOrder[order[i] - 1]);
            //}

            ////Lati@s-----------------------------------------------------------------------------------
            //outLs.Add("\r\n\r\n" + divider);
            //outLs.Add("   Lati@s choices");
            //outLs.Add(divider);
            //int latios = byteToInt(rom[addy("242ba7")]) + byteToInt(rom[addy("242ba8")]) * 256;
            //int latias = byteToInt(rom[addy("242bba")]) + byteToInt(rom[addy("242bbb")]) * 256;
            //outLs.Add(decodeText(addy("5ee14b"), 3) + "\t" + pkmn[latias - 1]);
            //outLs.Add(decodeText(addy("5ee14f"), 4) + "\t" + pkmn[latios - 1]);


            ////Pickup Item List------------------------------------------------------------------------------
            //outLs.Add("\r\n\r\n" + divider);
            //outLs.Add("   Pickup Item List");
            //outLs.Add(divider);



            //ptr = addy("31c440");
            //String[] itemList = ak.getItemList();
            //int item;
            //String[] pickup = new String[29];
            //for (int i = 0; i < 29; i++, ptr += 2)
            //{
            //    item = byteToInt(rom[ptr]) + byteToInt(rom[ptr + 1]) * 256;
            //    pickup[i] = itemList[item];
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    outLs.Add("L:" + ((10 * i) + 1) + "-" + (10 * (i + 1)));
            //    outLs.Add("  30% - " + pickup[i]);
            //    outLs.print("  10% - " + pickup[i + 1]);
            //    for (int j = 2; j < 7; j++)
            //    {
            //        outLs.print(", " + pickup[i + j]);
            //    } outLs.Add("");
            //    outLs.Add("   5% - " + pickup[i + 7]);
            //    outLs.Add("   3% - " + pickup[i + 8]);
            //    outLs.Add("   1% - " + pickup[i + 18] + ", " + pickup[i + 19]);
            //}
            #endregion

            return outLs.ToArray();
        }
    }
}