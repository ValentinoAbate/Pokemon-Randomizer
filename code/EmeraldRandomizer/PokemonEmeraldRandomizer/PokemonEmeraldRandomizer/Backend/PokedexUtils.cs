using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    //A utility class containing various tables to of pokedex data
    public static class PokedexUtils
    {
        private static readonly int[] species_to_dex =
        {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
            41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,
            78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,
            111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,
            139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166,
            167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,
            195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
            223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,
            251,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            252,253,254,255,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,290,291,292,276,
            277,285,286,327,278,279,283,284,320,321,300,301,352,343,344,299,324,302,339,340,370,341,342,349,350,318,319,328,
            329,330,296,297,309,310,322,323,363,364,365,331,332,361,362,337,338,298,325,326,311,312,303,307,308,333,334,360,
            355,356,315,287,288,289,316,317,357,293,294,295,366,367,368,359,353,354,336,335,369,304,305,306,351,313,314,345,
            346,347,348,280,281,282,371,372,373,374,375,376,377,378,379,382,383,384,380,381,385,386,358
        };
        private static readonly int[] dex_to_species =
        {
            0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
            41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,
            78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,
            111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,
            139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166,
            167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,
            195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
            223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,
            251,
            277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,297,298,299,300,304,305,309,310,
            392,393,394,311,312,306,307,364,365,366,301,302,303,370,371,372,335,336,350,320,315,316,322,355,382,383,384,356,
            357,337,338,353,354,386,387,363,367,368,330,331,313,314,339,340,321,351,352,308,332,333,334,344,345,358,359,380,
            379,348,349,323,324,326,327,318,319,388,389,390,391,328,329,385,317,377,378,361,362,369,411,376,360,346,347,341,
            342,343,373,374,375,381,325,395,396,397,398,399,400,401,402,403,407,408,404,405,406,409,410
        };
        private static readonly string[] menu_name = new string[]
        {
            "000 - Random","001 - Bulbasaur","002 - Ivysaur","003 - Venusaur","004 - Charmander","005 - Charmeleon",
            "006 - Charizard","007 - Squirtle","008 - Wartortle","009 - Blastoise","010 - Caterpie","011 - Metapod","012 - Butterfree",
            "013 - Weedle","014 - Kakuna","015 - Beedrill","016 - Pidgey","017 - Pidgeotto","018 - Pidgeot","019 - Rattata","020 - Raticate",
            "021 - Spearow","022 - Fearow","023 - Ekans","024 - Arbok","025 - Pikachu","026 - Raichu","027 - Sandshrew","028 - Sandslash",
            "029 - Nidoranf","030 - Nidorina","031 - Nidoqueen","032 - Nidoranm","033 - Nidorino","034 - Nidoking","035 - Clefairy",
            "036 - Clefable","037 - Vulpix","038 - Ninetales","039 - Jigglypuff","040 - Wigglytuff","041 - Zubat","042 - Golbat",
            "043 - Oddish","044 - Gloom","045 - Vileplume","046 - Paras","047 - Parasect","048 - Venonat","049 - Venomoth","050 - Diglett",
            "051 - Dugtrio","052 - Meowth","053 - Persian","054 - Psyduck","055 - Golduck","056 - Mankey","057 - Primeape","058 - Growlithe",
            "059 - Arcanine","060 - Poliwag","061 - Poliwhirl","062 - Poliwrath","063 - Abra","064 - Kadabra","065 - Alakazam","066 - Machop",
            "067 - Machoke","068 - Machamp","069 - Bellsprout","070 - Weepinbell","071 - Victreebel","072 - Tentacool","073 - Tentacruel",
            "074 - Geodude","075 - Graveler","076 - Golem","077 - Ponyta","078 - Rapidash","079 - Slowpoke","080 - Slowbro","081 - Magnemite",
            "082 - Magneton","083 - Farfetch'd","084 - Doduo","085 - Dodrio","086 - Seel","087 - Dewgong","088 - Grimer","089 - Muk",
            "090 - Shellder","091 - Cloyster","092 - Gastly","093 - Haunter","094 - Gengar","095 - Onix","096 - Drowzee","097 - Hypno",
            "098 - Krabby","099 - Kingler","100 - Voltorb","101 - Electrode","102 - Exeggcute","103 - Exeggutor","104 - Cubone","105 - Marowak",
            "106 - Hitmonlee","107 - Hitmonchan","108 - Lickitung","109 - Koffing","110 - Weezing","111 - Rhyhorn","112 - Rhydon",
            "113 - Chansey","114 - Tangela","115 - Kangaskhan","116 - Horsea","117 - Seadra","118 - Goldeen","119 - Seaking","120 - Staryu",
            "121 - Starmie","122 - Mr.Mime","123 - Scyther","124 - Jynx","125 - Electabuzz","126 - Magmar","127 - Pinsir","128 - Tauros",
            "129 - Magikarp","130 - Gyarados","131 - Lapras","132 - Ditto","133 - Eevee","134 - Vaporeon","135 - Jolteon","136 - Flareon",
            "137 - Porygon","138 - Omanyte","139 - Omastar","140 - Kabuto","141 - Kabutops","142 - Aerodactyl","143 - Snorlax","144 - Articuno",
            "145 - Zapdos","146 - Moltres","147 - Dratini","148 - Dragonair","149 - Dragonite","150 - Mewtwo","151 - Mew","152 - Chikorita",
            "153 - Bayleef","154 - Meganium","155 - Cyndaquil","156 - Quilava","157 - Typhlosion","158 - Totodile","159 - Croconaw",
            "160 - Feraligatr","161 - Sentret","162 - Furret","163 - Hoothoot","164 - Noctowl","165 - Ledyba","166 - Ledian","167 - Spinarak",
            "168 - Ariados","169 - Crobat","170 - Chinchou","171 - Lanturn","172 - Pichu","173 - Cleffa","174 - Igglybuff","175 - Togepi",
            "176 - Togetic","177 - Natu","178 - Xatu","179 - Mareep","180 - Flaaffy","181 - Ampharos","182 - Bellossom","183 - Marill",
            "184 - Azumarill","185 - Sudowoodo","186 - Politoed","187 - Hoppip","188 - Skiploom","189 - Jumpluff","190 - Aipom","191 - Sunkern",
            "192 - Sunflora","193 - Yanma","194 - Wooper","195 - Quagsire","196 - Espeon","197 - Umbreon","198 - Murkrow","199 - Slowking",
            "200 - Misdreavus","201 - Unown","202 - Wobbuffet","203 - Girafarig","204 - Pineco","205 - Forretress","206 - Dunsparce",
            "207 - Gligar","208 - Steelix","209 - Snubbull","210 - Granbull","211 - Qwilfish","212 - Scizor","213 - Shuckle","214 - Heracross",
            "215 - Sneasel","216 - Teddiursa","217 - Ursaring","218 - Slugma","219 - Magcargo","220 - Swinub","221 - Piloswine","222 - Corsola",
            "223 - Remoraid","224 - Octillery","225 - Delibird","226 - Mantine","227 - Skarmory","228 - Houndour","229 - Houndoom",
            "230 - Kingdra","231 - Phanpy","232 - Donphan","233 - Porygon","234 - Stantler","235 - Smeargle","236 - Tyrogue","237 - Hitmontop",
            "238 - Smoochum","239 - Elekid","240 - Magby","241 - Miltank","242 - Blissey","243 - Raikou","244 - Entei","245 - Suicune",
            "246 - Larvitar","247 - Pupitar","248 - Tyranitar","249 - Lugia","250 - Ho-oh","251 - Celebi","252 - Treecko","253 - Grovyle",
            "254 - Sceptile","255 - Torchic","256 - Combusken","257 - Blaziken","258 - Mudkip","259 - Marshtomp","260 - Swampert",
            "261 - Poochyena","262 - Mightyena","263 - Zigzagoon","264 - Linoone","265 - Wurmple","266 - Silcoon","267 - Beautifly",
            "268 - Cascoon","269 - Dustox","270 - Lotad","271 - Lombre","272 - Ludicolo","273 - Seedot","274 - Nuzleaf","275 - Shiftry",
            "276 - Taillow","277 - Swellow","278 - Wingull","279 - Pelipper","280 - Ralts","281 - Kirlia","282 - Gardevoir","283 - Surskit",
            "284 - Masquerain","285 - Shroomish","286 - Breloom","287 - Slakoth","288 - Vigoroth","289 - Slaking","290 - Nincada",
            "291 - Ninjask","292 - Shedinja","293 - Whismur","294 - Loudred","295 - Exploud","296 - Makuhita","297 - Hariyama","298 - Azurill",
            "299 - Nosepass","300 - Skitty","301 - Delcatty","302 - Sableye","303 - Mawile","304 - Aron","305 - Lairon","306 - Aggron",
            "307 - Meditite","308 - Medicham","309 - Electrike","310 - Manectric","311 - Plusle","312 - Minun","313 - Volbeat","314 - Illumise",
            "315 - Roselia","316 - Gulpin","317 - Swalot","318 - Carvanha","319 - Sharpedo","320 - Wailmer","321 - Wailord","322 - Numel",
            "323 - Camerupt","324 - Torkoal","325 - Spoink","326 - Grumpig","327 - Spinda","328 - Trapinch","329 - Vibrava","330 - Flygon",
            "331 - Cacnea","332 - Cacturne","333 - Swablu","334 - Altaria","335 - Zangoose","336 - Seviper","337 - Lunatone","338 - Solrock",
            "339 - Barboach","340 - Whiscash","341 - Corphish","342 - Crawdaunt","343 - Baltoy","344 - Claydol","345 - Lileep","346 - Cradily",
            "347 - Anorith","348 - Armaldo","349 - Feebas","350 - Milotic","351 - Castform","352 - Kecleon","353 - Shuppet","354 - Banette",
            "355 - Duskull","356 - Dusclops","357 - Tropius","358 - Chimecho","359 - Absol","360 - Wynaut","361 - Snorunt","362 - Glalie",
            "363 - Spheal","364 - Sealeo","365 - Walrein","366 - Clamperl","367 - Huntail","368 - Gorebyss","369 - Relicanth","370 - Luvdisc",
            "371 - Bagon","372 - Shelgon","373 - Salamence","374 - Beldum","375 - Metang","376 - Metagross","377 - Regirock","378 - Regice",
            "379 - Registeel","380 - Latias","381 - Latios","382 - Kyogre","383 - Groudon","384 - Rayquaza","385 - Jirachi","386 - Deoxys"
        };
        public static int PokedexIndex(PokemonSpecies s)
        {
            return species_to_dex[(int)s];
        }
        public static PokemonSpecies DexToSpecies(int dexInd)
        {
            return (PokemonSpecies)dex_to_species[dexInd];
        }
        public static string MenuName(int dexInd)
        {
            return menu_name[dexInd];
        }
        public static string MenuName(PokemonSpecies s)
        {
            return MenuName(species_to_dex[(int)s]);
        }
        public static string DexName(int dexInd)
        {
            return menu_name[dexInd].Replace(" - ",".").ToUpper();
        }
        public static string DexName(PokemonSpecies s)
        {
            return DexName(species_to_dex[(int)s]);
        }
    }
}
