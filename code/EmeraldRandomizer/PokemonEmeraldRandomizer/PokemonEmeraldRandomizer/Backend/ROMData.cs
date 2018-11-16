using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class ROMData
    {
        public byte[] RawROM { get; }
        public Pokemon[] Starters { get; set; }
        public List<PokemonDefinition> Pokemon { get; set; }
        public List<Trainer> Trainers { get; set; }
        public ROMData(byte[] rawROM)
        {
            RawROM = rawROM;
        }
        public override string ToString()
        {
            const string divider = "=======================================================================" +
                                   "========================================================================";
            List<string> outLs = new List<string>();
            //String[] pkmn = ak.getPokemonListGameOrder();

            //String[] arPkmn = new ArrayKeeper().getPokemonListGameOrder();
            //StringBuilder sb;

            ////PokeData-----------------------------------------------------------------------
            //int ptr = addy("3203e8");
            //int[] arData = new int[28];
            //PokeData pkdt;

            //outLs.Add(divider);
            //outLs.Add("   Pokémon Info List");
            //outLs.Add(divider);
            //outLs.Add(("Pkmn Name      |  HP  AT  DF  SP  SA  SD   |       EPs      |" +
            //    " Type(s) |  Ability 1     | Ability 2     | Held Item 1   | Held Item 2   | "));
            //outLs.Add(("---------------------------------------------------------" +
            //        "----------------------------------------------------------------------------"));

            //String[] actualOrder = new String[411];
            //for (int i = 0; i < 411; i++, ptr += 28)
            //{
            //    if (i >= 251 && i < 276) continue;
            //    for (int p = 0; p < 28; p++)
            //    {
            //        arData[p] = byteToInt(rom[ptr + p]);
            //    }
            //    sb = new StringBuilder(String.format("%-10s", arPkmn[i]));
            //    pkdt = new PokeData(arData);
            //    actualOrder[i] = sb.toString() + " | " + pkdt.toString();
            //}
            //int[] order = ak.getPkmnDexToGameTranscription();
            //for (int i = 0; i < order.length; i++)
            //{
            //    outLs.Add(String.format("%03d.", i + 1) + actualOrder[order[i] - 1]);
            //}

            ////----------------------------TMs
            //outLs.Add("\r\n\r\n" + divider);
            //outLs.Add("   TM/HM Compatability List");
            //outLs.Add(divider);
            //ptr = addy("615b94");
            //String[] atList = ak.getAttackListText();
            //int tm = 0, tm2 = 0;

            //for (int i = 0; i < 25; i++, ptr += 2)
            //{
            //    tm = byteToInt(rom[ptr]) + byteToInt(rom[ptr + 1]) * 256;
            //    tm2 = byteToInt(rom[ptr + 50]) + byteToInt(rom[ptr + 51]) * 256;
            //    outLs.printf("TM%02d - %-10s\tTM%02d - %-10s\r\n", (i + 1), atList[tm], (i + 26), atList[tm2]);
            //}
            ////TM compat-----------------------------------------------------------------------------------
            //outLs.Add("\r\n\r\n" + divider);
            //outLs.Add("   TM/HM Compatability List");
            //outLs.Add(divider);
            //ptr = addy("31e8a0");
            //String header = "\r\n\r\n               |01  03  05|  07  09  |11  13  15|  17  19  |21  23  25|" +
            //            "  27  29  |31  33  35|  37  39  |41  43  45|  47  49  |01  03  05|  07  \r\n" +
            //                "               |   02  04 |06  08  10|  12  14  |16  18  20|  22  24  |" +
            //            "26  28  30|  32  34  |36  38  40|  42  44  |46  48  50|  02  04  |06  08\r\n" +
            //                "-----------------------------------------------------------------------" +
            //            "------------------------------------------------------------------------";
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
            string ret = string.Empty;
            foreach(string line in outLs)
            {
                ret += (line + "\n");
            }
            return ret;
        }
    }
}