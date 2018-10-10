/*
 * Pokemon Emerald Randomizer, v.2.2
 * Date of release: 13 April 2014
 * Author: Artemis251
 * Thanks for takin' a peek!
 */

package emeraldrandomizer;

import java.util.ArrayList;

public class ArrayKeeper {
    public ArrayKeeper(){
        //empty
    }

    public int[] getTrainersFullEvo(){
        return new int[]{
            29,32,                              //aqua
            33,                                 //aqua admin
            260,261,262,263,                    //elite 4
            264,265,266,267,268,269,270,271,    //gym
            334,                                //champion
            513,                                //magma
            518,                                //wally
            600,601,                            //magma admin
            656,657,658,659,                    //wally
            733,                                //magma admin
            769,770,771,772,773,774,775,776,    //gym
              777,778,779,780,781,782,783,784,  
              785,786,787,788,789,790,791,792,
              793,794,795,796,797,798,799,800,
            803                                 //steven
        };
    }

    public int[] getMaleGenderOptions(){
        return new int[]{
            29,30,31,113,115,124,238,241,242,387,407,81,82,100,101,120,121,137,233,303,348,349,318,319,398,399,400,
            132,144,145,146,150,151,201,243,244,245,249,250,251,401,402,403,404,405,406,407,408,409,410
        };
    }

    public String[][] getJohtoTextFixes(){
        return new String[][]{
            {   "absurd POK@MON#[1] caught your eye!$You're as sharp as ever!$So [1] is your choice?*",
                "silent POK@MON#[1] caught your eye!$You're as sharp as ever!$So [1] is your choice?*",
                "classy POK@MON#[1] caught your eye!$You're as sharp as ever!$So [1] is your choice?*"},
            {   "dinky POK@MON#[1] is your choice!$You know how to pick a good one.$So, you'll take [1]?*",
                "goopy POK@MON#[1] is your choice!$You know how to pick a good one.$So, you'll take [1]?*",
                "whiny POK@MON#[1] is your choice!$You know how to pick a good one.$So, you'll take [1]?*" },
            {   "foreign POK@MON#[1] is your choice!$You sure know what you're doing.$So, you'll take [1]?*",
                "maddest POK@MON#[1] is your choice!$You sure know what you're doing.$So, you'll take [1]?*",
                "yawning POK@MON#[1] is your choice!$You sure know what you're doing.$So, you'll take [1]?*" }
        };
    }

    public String[][] getTutorTextFixes(){
        return new String[][]{
            {   "Even with my best [1],#I couldn't even catch his eye.$Please, let me teach your POK@MON#the move [1]!*"},
            {   "learn#[1]?*"},//double-edge
            {   "mock#the styles and speech of the elderly^folks of this town.$What do you say, young one?#"+
                        "Would you agree to it if I offered^to teach the move [1]?*"},
            {   "[1] is a move of great depth.$Could you use it as perfectly#as I do~?*"},
            {   "Boo! I wanted to teach my move#to your POK@MON!*"},//mimic
            {   " complain incessantly?$If you want, I can teach your POK@MON#the move [1].$Money won't appear, but your"+
                        " POK@MON#will complain.*",
                "'re poor?$If you want, I can teach your POK@MON#the move [1].$Money won't appear, but your POK@MON#"+
                        "will experience poverty. Yes?*",
                " defy gravity?$If you want, I can teach your POK@MON#the move [1].$Money won't appear, but your POK@MON#"+
                        "will float around. Yes?*"},
            {   "majorly gripes#like a [1], all sorts^of nice things happen.*",
                "loses money#by using [1], all sorts^of nice things happen.*",
                "ignores physics#like a [1], all sorts^of nice things happen.*"},//metronome
            {   "huge hit.$Seriously, I'm going to cause#a big [1] of pain!$If you overheard that, I'll gladly#teach [1] to your POK@MON!*",
                "huge git.$Seriously, I'm going to cause#a big [1] of idiocy!$If you overheard that, I'll gladly#teach [1] to your POK@MON!*",
                "beggar.$Seriously, I'm going to cause#a big [1] of debt!$If you overheard that, I'll gladly#teach [1] to your POK@MON!*"},
            {   "[1] it is!#Which POK@MON wants an injury?*",
                "[1] it is!#Which POK@MON wants to be dumb?*",
                "[1] it is!#Which POK@MON wants to be poor?*"},
            {   "[1], but I've^yet to make my own [1]~$Maybe it's because deep down,#I'd rather stay here~*"},//explosion
            {   "fully explore it all.  $Of course it's not possible.#Giggle~$I know! Would you be interested in#"+
                       "having a POK@MON learn [1]?*"},
            {   "to ^teach [1]?*"},
            {   "fool"},  //substitute
            {   "[1]?*"},
            {   "[1]!*"},//dynamicpunch
            {   "forward^without changing direction?$I might even be able to fly#that way.$Anyway, do you think your POK@MON#"+
                        "will want a new attack?$I can teach one#[1] if you'd like.*",
                "forward^without changing direction?$I might even be able to fall#that way.$Anyway, do you think your POK@MON"+
                        "#will want a new attack?$I can teach one#[1] if you'd like.*",
                "forward^without changing direction?$I might even be able to cartwheel#that way.$Anyway, do you think your POK@MO"+
                        "N#will want a new attack?$I can teach one#[1] if you'd like.*"},//rollout
            {   "catchphrase#like that when the CHAIRMAN^chewed me out.$He's just jealous of my amazing moves.$"+
                        "If you'd like, I'll teach the move #[1] to a POK@MON of yours.*"},
            {   "Okay, which POK@MON wants to#learn [1]?*"},
            {   "being loud.*"},//swagger
            {   "#caffeine to stay awake.$She should just nap like I do,#and [1].$I can teach your POK@MON how to#[1] instead. Interested?*"},
            {   "even#as I [1]~*"},//sleep talk
            {   "act cuddly and cute.$It's a FUZZY-type move, and it is#wickedly cool.$It's called [1].#Want me to teach it to a POK@MON?*",
                "spin around in place.$It's a DIZZY-type move, and it's#wickedly cool.$It's called [1].#Want me to teach it to a POK@MON?*",
                "hack into databases.$It's a VIRUS-type move, and it is#wickedly cool.$It's called [1].#Want me to teach it to a POK@MON?*"},
            {   "is actually effective!*",
                "ever makes the foe comatose!*",
                "pierces the heavens!*"}//
        };
    }

    public int[] getTutorTextFixAddresses(){
        //rollout,fury cutter,double-edge,dynamicpunch,explosion,metronome, mimic, sleep talk, substitute, swagger
        return new int[]{
            addy("2c7c03"),addy("2c7cb3"), //doub
            addy("2c72e2"),addy("2c73f6"),addy("2c737f"), //mimic
            addy("2c74cb"),addy("2c75c4"),//metro
            addy("2c7d77"),addy("2c7e46"),addy("2c7eaa"),//expl
            addy("2c784c"),addy("2c7933"),addy("2c78ee"),//sub
            addy("2c7ac6"),addy("2c7b80"),//dynam
            addy("2c700d"),//roll
            addy("2c6e8d"),addy("2c6f66"),addy("2c6fce"),//swagg
            addy("2c764f"),addy("2c77aa"),//sleep
            addy("2c719f"),addy("2c7299")//fury c
        };
    }

    public int[] getTutorTextFixCount(){
        return new int[]{
            0,0,0,0,2,0,0,3,2,0,
            0,0,3,0,3,2,1,0,0,0,
            0,0,0,3,2,0,0,0,0,2
        };
    }

    public int[] getTMTextFixAddresses(){
        return new int[]{
            addy("2256ed"),
            addy("221a60"),
            addy("1f2820"),addy("1f2880"),
            addy("1fd194"),
            addy("1ed83f"),
            addy("217a91"),
            addy("1e0284"),
            addy("22ae24"),
            addy("226a21"),
            addy("20e85d"),
            addy("1fe14a"),addy("1fe1f4"),//36
            addy("2138d8"),
            addy("216ffa"),
            addy("208c84"),//41
            addy("206267"),
            addy("272e40"),
            addy("22ddc5"),
            addy("23d0f6"),addy("23d145"),
            addy("1ff46f")};
    }

    public int[] getTMTextFixCount(){
        return new int[]{0,0,1,1,2,0,0,1,1,1,
                         0,0,0,0,0,0,0,0,0,0,
                         0,0,0,1,0,0,0,1,0,0,
                         1,0,0,1,0,2,0,0,1,1,
                         1,1,1,0,0,0,1,0,2,1 };
    }
    
    public String[][] getTMTextFixes(){
        return new String[][]{//{""},{""},
        {   "[1].$In use, it will cause the foe to#question its trainer's motives.*",//3
            "[1].$In use, it will cause the foe to#swoon uncontrollably.*",
            "[1].$In use, it will cause the foe to#forget its own name, oddly enough.*"},
        {   "[1]!$TATE: It lifts and~#LIZA: It separates!$TATE: It's a move that's perfect~#LIZA: For bouncy POK@MON!*",//4
            "[1]!$TATE: It slices! It dices!#LIZA: It makes Julienne fries!$TATE: It's a move that's perfect~#LIZA: For TV ads!*",
            "[1]!$TATE: It huffs and~#LIZA: It puffs!$TATE: It's a move that's perfect~#LIZA: For blowing houses down!*"},
        {   "pout","lose","trip"},
        {   "[1].#It inspires POK@MON.*",
            "[1].#It trounces POK@MON.*",
            "[1].#It camoflauges POK@MON.*"},//5
        {   "[1].$It's a move that caused trauma#to innocent kids.*",
            "[1].$It's a move that's banned in#many tournaments.*",
            "[1].$It's a move that moves movers#who move movers.*"},//8
        {   "filling my mind with lies,#then spreading them as rumors!$You can have this, so you try it out!$" +
                    "Use it on a POK@MON, and it will learn#a move for spreading slander.*",
            "using phony accents, then#calling random phone numbers!  $You can have this, so you try it out!$" +
                    "Use it on a POK@MON, and it will learn#a move for crank phone calls.*",
            "amassing POK@MON eggs, then#releasing the babies into the wild!$You can have this, so you try it " +
                    "out!$Use it on a POK@MON, and it will learn#a move for mass-breeding.*"},//9
        {   "[1] is a move that#resonates with POK@MON.*"},//10
        {   "[1]!$You've earned it!*"},//24
        {   "#make my big brother love me again~*",
            "#deactivate my big brother~*",
            "#genetically enhance my big brother~*"},//28
        {   "[1]! It's a move#so awkward that I can't describe it.*",
            "[1]! It's a move#so funky that I can't describe it.*",
            "[1]! It's a move#so trashy that I can't describe it.*"},//31
        {   "[1].$It's a lame move that always #uses 1 PP! You can count on it!*",
            "[1].$It's a worthy move that always#amuses! You can count on it!*",
            "[1].$It's a move that needs to be#nerfed! You can count on it!*"},//34
        {   "[1] is at the peak#of popularity. It's great.$Hunh? You're telling me that you don't#know about [1]?"+
                    "$That's outright pitiful.#I'll give you one.*"},
        {   "the move [1].$But moves like#[B1FD02B2]^are in, too.*"},//36
        {   "[1].$It's an enigma exactly what it can#do, but trust me. It's good.*",
            "[1].$A friend of mine used it and#now his POK@MON glow in the dark.*",
            "[1].$It makes your POK@MON study#theoretical physics and astrology.*"},//39
        {   "[1].$Its cheapness~#Nothing can possibly avoid it.*",
            "[1].$Its irony~#No POK@MON should be able to see it.*",
            "[1].$Its odor~#No POK@MON should smell that badly.*"},//40
        {   "[1], you hear?$Like, it won't let the other guy#eat the same meal twice in a row, see?$"+
                    "Hey, now, you listen here, like,#I'm not making a dinner for you!*",
            "[1], you hear?$Like, it won't let the other guy#succeed in world domination, see?$"+
                    "Hey, now, you listen here, like,#I'm not making heinous plots for you!*",
            "[1], you hear?$Like, it won't let the other guy#buy the same item twice in a row, see?$"+
                    "Hey, now, you listen here, like,#I'm not purchasing junk for you!*"},//41
        {   "[1].$It doubles the power of moves if#the POK@MON is starving, blind,^or dead.$"+
                "It might be able to turn a bad#situation into an advantage.*",
            "[1].$It doubles the power of money if#the POK@MON invests its^savings wisely. $"+
                "It might be able to turn a bad#situation into an advantage.*",
            "[1].$It doubles the cost of healthcare#if the POK@MON is poisoned, paralyzed,^o"+
                "r burned.$It might be able to turn a bad#situation worse.*"},//42
        {   "move SECRET POWER?#Our group, we love SECRET POWER.$If you get a POK@MON that knows it,#"+
                    "come back and show it to me.$We'll accept you as a member and sell#"+
                    "you good stuff in secrecy.*"},//43
        {   "Oh, thank you.$You went through all this trouble to#deliver that. I need to thank you.$"+
                    "Let me see~#I'll give you this TM.$It contains my favorite move,#[1].*"},//47
        {   "steal it from "},
        {   "[1] steals the soul from#certain opponents before they can^turn to the dark side.*",
            "[1] steals the ideas from#certain developers before they can^copyright their ideas.*",
            "[1] steals the lyrics from#certain performers before they can^sing a karaoke song.*"},//49
        {   "[1].$That move was developed by a shady#rockstar.$But it also puts a catchy song in#"+
                    "your head when you use it. It might not^be suitable for longer battles.*",
            "[1].$That move essentially starves#the opponent.$But it also impacts the diet of#th"+
                    "e POK@MON using it. It might not^be suitable for longer battles.*",
            "[1].$That move makes the opponent#look hideous.$But it also sharply cuts the hair#"+
                    "of the POK@MON using it. It might not^be suitable for contest battles.*"}//50
        };
    }

    public int[] getFrontierEVs(){
        return new int[]{3,5,6,9,10,12,17,18,20,24,33,34,36,40,48,  //255/255 spreads
                         7,11,13,15,19,21,22,25,26,28,35,37,38,41,42,44,49,50,52,56};   //170/170/170 spreads
    }

    public int[] getSpecialEncounterSlots(){
        return new int[]{
            20,71,72,73,97,96, //safari zone
            106,107,108,109,   //mirage tower
            65,66,67,68,69,    //cave of origin
            116,               //mirage island
            114                //not sure where this is...
        };
    }


    public int[] getEncounterSlotAddresses(){
        return new int[]{
         5572582,5572638,5572770,5572902,5573034,5573110,5573242,5573402,5573458,5573514,
         5573674,5573730,5573862,5573994,5574070,5574126,5574182,5574238,5574294,5574350,
         5574406,5574462,5574490,5574566,5574650,5574706,5574838,5574894,5574970,5575046,
         5575122,5575198,5575330,5575386,5575518,5575650,5575782,5575858,5575990,5576046,
         5576102,5576158,5576214,5576326,5576382,5576438,5576514,5576590,5576666,5576742, //50
         5576874,5576950,5577026,5577102,5577178,5577254,5577330,5577386,5577442,5577498,
         5577554,5577610,5577742,5577874,5577930,5578006,5578062,5578118,5578174,5578230,
         5578286,5578342,5578474,5578558,5578690,5578774,5578906,5579038,5579170,5576270,
         5579302,5579358,5579414,5579546,5579678,5579754,5579830,5579906,5579982,5580058,
         5580134,5580210,5580294,5580350,5580426,5580482,
        addy("5526fa"), //addy("552732"),addy("55274e"), covered by 27 => 10+5+12 - surf, grass, fish
        addy("55277e"), //addy("5527b6"),  safari zone (this and the one above)
        addy("5527d2"),addy("55280a"),                                                    //100
        addy("552842"),addy("55287a"),addy("5528b2"),addy("5528ea"),addy("552922"),addy("55295a"),
        addy("552992"),addy("5529ca"),addy("552a02"),addy("552a3a"), //mirage tower (this line) -- 110
        addy("552a72"),addy("552aaa"),addy("552ae2"),addy("552b1a"),
        addy("552d12"),
        addy("5525ce"),
        addy("551872")      //mirage island
        };
    }

    public int[] getEncounterSlotLengths(){ //12 - grass/cave; 5 - surf;  10 - fishing;  5 - rock smash
         return new int[]{12,27,27,27,15,27,32,12,12,
             32,12,27,27,15,12,12,12,12,12,12,
             12,5,15,17,12,27,12,15,15,15,15,
             27,12,27,27,27,15,27,12,12,12,12,
             12,12,12,15,15,15,15,15,15,15,15,
             15,15,15,12,12,12,12,12,27,27,12,
             15,12,12,12,12,12,12,27,17,27,17,
             27,27,27,27,12,12,12,27,27,15,15,
             15,15,15,15,15,5,12,15,12,12,
             27,17,                 //safari zone 5,6
             12,12,                 //team magma hideout
             12,12,12,12,12,12,12,  //team magma hideout, mirage tower
             12,12,12,12,12,12,12,  //mirage tower, desert underpass, sketch, alter
                                    //...8 altering cave options skipped
             12,                    //?? Golbats and Solrocks
             12,                    //Shoal Cave (ice)
             12                     //mirage island
         };
    }

    public String[][] getTrainerTitles(){
        return new String[][]{
            {"HAIR HATTER","YOUNG GRAMPA","DIM NEIGHBOR","BIRCH SPROUT","VIRAL RIVAL"},     //pkmn trainer
            {"MARCHER","COORDINATOR","DIM NEIGHBOR","BIRCH SPROUT","VIRAL RIVAL"},          //pkmn trainer
            {"LOST MAN","BROCK CLONE","LEPRECHAUN","RUGGED MAN","HIKESMITH"},               //hiker
            {"HAMBURGLER","TEAM RAMROD","BANDANA BAND","SCURVY CUR","POOL CLEANER"},        //team aqua
            {"<> LOVE GURU","DITTO PIMP","EGG CREATOR","APRON MODEL","BABY BOOMER"},        //breeder
            {"BALL FLOATER","ALGAE HAIR","WARMTRAINER","CRUELTRAINER","SMUG JERK"},         //cooltrainer male
            {"CAGE TRAINER","THINKER","<> POACHER","WINGMAN","CAGE AND"},                   //bird keeper
            {"EV MASTER","UTILITY BELT","SMOGON GRAD","BULBAPEDIAN","DUAL WIELDER"},        //collector
            {"SQUATTER","BACKSTROKER","DUCKWALKER","MOIST MAN","SPEEDO MODEL"},             //swimmer male
            {"TEAM AWESOME","LAND HIPPIE","BROIL BUNCH","RED RIDER","ROUGE ROGUE"},         //team magma
            {"RETIREE","KUNG FOOL","FLOODPANTS","SENILE GURU","WRINKLESKIN"},               //expert
            {"LITER LEADER","SWIM COACH","FIRST MATE","VEST VYER","MACGUFFIN"},             //aqua admin
            {"FOOT CLAN","CRAZY LEGS","OFF-BALANCE","KICKPUNCHER","RYU WANNABE"},           //black belt
            {"RAINING KING","THE BIG MAN","CURRENT BOSS","FLOOD FATHER","BANDANA SUIT"},    //aqua leader
            {"WICCA WALKER","PAGAN LADY","PSYCH MAJOR","HALLOWEENIE","WHICH WITCH"},        //hex maniac
            {"DAZED DAME","SMELLOMANCER","SLEEPWALKER","FAERIE LOVER","TWILIGHT FAN"},      //aroma lady
            {"MOSTLY BLIND","PARTY RUINER","ALL-SEER","LOST TOURIST","BIG MONOCLE"},        //ruin maniac
            {"CANDID CREEP","<> GONE WILD","STALKER","REALITY TV","PAPRAZZI"},              //reporter
            {"SPLASH ZONE","YOUTH TUBE","TUUUUUUUBES","INFLATABLE","BALLERINA"},            //tuber female
            {"SHARK BAIT","BUOY BOY","OUTER TUBER","POTATO","SATURN MIMIC"},                //tuber male
            {"OVERDRESSED","PICNIC PAL","REGAL GAL","FLOPPY HAT","BASKETCASE"},             //lady
            {"POSE STRIKER","SALON BINGER","NARCISSIST","TOO SEXY","PHOTOSHOPPER"},         //beauty
            {"RICH SUITOR","BAD HEIR","PLAYBOY","ZILLIONAIRE","SNARKYPANTS"},               //rich boy
            {"DRAGONBORN","CHAR-MAN-DER","DINO SIR","HALF-BREED","WILD THING"},             //pokemaniac
            {"MUSKY BUSKER","TOPLESS TOOL","FREEBIRDER","FENDERBENDER","GROOVY GUY"},       //guitarist
            {"NICE BLAZER","FLAMER TROLL","HUMAN TORCH","ARSON SON","PYRO BRO"},            //kindler
            {"SPAWN CAMPER","KNOT TIER","HAPPY CAMPER","PREPARED","BOY SCOOT"},             //camper
            {"LOST KID","FUND RAISER","CAMPOFLAUGER","THIN MINTER","GIRL SCOOT"},           //picknicker
            {"CAGESHINS","BUG-EYED BOY","INSECTIVORE","EXTERMINATOR","NET WORKER"},         //bug maniac
            {"PSYCHONAUT","SIDEKICK","ONESIE CREEP","SHOELESS SAP","NUCLEAR HAND"},         //psychic
            {"RAPSCALLION","OLD RASCAL","HABERDASHER","MOUSTACHER","BOWLER CHAP"},          //gentleman
            {"PRO FAINTER","ELITE NINETY","ONE OF FOUR","FINAL BOSS","ELITE POOR"},         //elite four
            {"FOLLOWER","UNION WORKER","MINI-BOSS","BADGE GUARD","GYM JUNKIE"},             //leader
            {"NERD HERDER","TECH SUPPORT","WHIZ KID","SHRIMP","POINDEXTER"},                //school kid
            {"SHORT SKIRTS","PEP SQUAD","TRUANT GIRLS","CHIC CHICKS","DROPOUT DUO"},        //sr. and jr.
            {"FAMILIAR","ELITE FIVE","LOSECURVE","BRAGGART","WEALTHY BUM"},                 //winstrate
            {"<> RESELLER","MERCHANDISER","POK@ FACE","CRAZY PARENT","<> OBSESSOR"},        //pokefan
            {"RATTATA RANK","LITTLE BRO","BOY OH BOY","SHORTS FAN","KIDDING KID"},          //youngster
            {"HUGGER","FLOURISHER","HAIR STYLER","CHAMP CHUMP","CREDITS WARD"},             //champion
            {"FISH POINTER","FISHY GUY","A LURING GUY","REEL MAN","CAST MEMBER"},           //fisherman
            {"FITNESS FOOL","BEEFCAKE","EX-EXERCISER","GYM TEACHER","TIME TRIALER"},        //triathlete
            {"SUPERHERO","DAFT POINTER","BALANCE ACT","CAPE BOSS","JAZZERCISER"},           //dragon tamer
            {"NARUTO FAN","SHORT NINJA","SWORD LOSER","DISSED GUISE","CAMO MASTER"},        //ninja boy
            {"MATRIX GAL","KARATE KID","LEAPSTER","FLYING GIRL","FLOATER"},                 //battle girl
            {"PUDDLER","GALOSHES GAL","UMBRELLARINA","DRIPDROPPER","BROLLY DOLLY"},         //parasol lady
            {"SCHWIMMER","FREESTYLER","SWIM SUITOR","SPLASH MAKER","WAVE TREADER"},         //swimmer female
            {"CLONES","DUPLICATES","EXTRA LIVES","PHOTOCOPIES","MIRROR IMAGE"},             //twins
            {"HULKSTER","STAY PUFT","SEE MAN","PETTY SEAMAN","MOON SAILOR"},                //sailor
            {"BALL FLOATER","ALGAE HAIR","WARMTRAINER","CRUELTRAINER","SMUG JERK"}, //dupe  //cooltrainer female
            {"SOL BADGUY","LAVA LOVA","GLOBE WARMER","CAPED CREEP","GAMMA MAGMA"},          //magma admin
//            {"GREEN PEACE","SICKLY SOD","INFECTED","CONTAGIOUS","HOSPITAL PAL"},            //pkmn trainer
            {"BFF","PLOTLINER","ROAMING GOOF","LIGHT THREAT","RANDOM RIVAL"},               //pkmn trainer
            {"LI'L BUGGER","RYE CATCHER","BEETLE BOY","PET POACHER","STRAW HATTER"},        //bug catcher
            {"POWER RANGER","LOOP ARTIST","KNOTTY HIKER","STYLUS ADEPT","<> BORROWER"},     //pkmn ranger
            {"SOILED EXEC","WIDOW PEAKER","FIRING BOSS","MINIMIZER","HEAD HONCHO"},         //magma leader
            {"SOPHOMORE","UNIFORMER","SASSY LASSY","LI'L TEAPOT","BALL DROPPER"},           //lass
            {"HUGGERNAUTS","CASSANOVAS","CHOKER CHUMS","AFFAIR SQUAD","LESS THAN 3"},       //young couple
            {"OLD PEOPLES","ELDER LOVERS","SR. LEAGUE","BINGO CHAMPS","DENTURE DUO"},       //old couple
            {"POOL FOOLS","ORPHANS","DRIFTERS","TREADPASSERS","WATER TRIBE"},               //sis and bro
            {"SALOON MAID","CEYLON MAID","CILAN MAIDEN","CONNOISSEUR","LILAC MANIAC"},      //salon maiden
            {"DUMB ACE","PARAJESTER","NiGHTS NUT","DOME FACE","DOME JOKER"},                //dome ace
            {"Z.Z. TOPPER","HUT MAVEN","POK@MON DI","CRAVEN MAVEN","TRIBAL ELDER"},         //palace maven
            {"ARENA MAROON","BOXED MIME","JUDO DODO","HIGH FIVER","FLUFFHEAD"},             //arena tycoon
            {"MOBSTER","ANGRY FARMER","HAT HEAD","FACTORY BRED","GEAR GUY"},                //factory head
            {"PUKE QUEEN","GOTH GURU","PIKE TEEN","EMO MASTER","BELT BOASTER"},             //pike queen
            {"PYRAMID DING","SGT. POINTER","HELMET HAIR","PYRAMIDIOT","ZIGGURAT ACE"},      //pyramid ace
            {"RANDOM RIVAL","HUMAN AI","AN OPPONENT","BORING FOE","NON-NPC"},               //pkmn trainer
        };
    }

    public int[][] getJohtoStarterAddys(){
        return new int[][]{
            {addy("1fa03a"),addy("1fa1b0"),addy("1fa1b5"),addy("1fa1b8")},      //chikorita
            {addy("1f9fd2"),addy("1fa06e"),addy("1fa073"),addy("1fa076")},      //cyndaquil
            {addy("1fa006"),addy("1fa10f"),addy("1fa114"),addy("1fa117")}       //totodile
        };
    }

    public int[][] getUniquePokemonAddys(){
        return new int[][]{
                 {addy("1ea783")},                                              //wynaut        0
                 {addy("1f56d6"),addy("1f56df")},                               //kecleon 1
                 {addy("211a1c"),addy("211ac6"),addy("211ad4"),addy("211a41"),
                          addy("211a44")},                                      //lileep
                 {addy("211a2e"),addy("211ae4"),addy("211ae7"),addy("211b69"),
                          addy("211b77")},                                      //anorith
                 {addy("222868"),addy("22286b"),addy("2228ed"),addy("2228fe")}, //beldum
                 {addy("22da06"),addy("22da0f"),addy("22da55")},                //regirock      5
                 {addy("2339ee"),addy("2339f5")},                               //electrode 1
                 {addy("233a3b"),addy("233a42")},                               //electrode 2
                 {addy("2377b2"),addy("2377b9")},                               //voltorb 1
                 {addy("2377ff"),addy("237806")},                               //voltorb 2
                 {addy("23784c"),addy("237853")},                               //voltorb 3     10
                 {addy("238f5c"),addy("238f65"),addy("238fab")},                //regice
                 {addy("23905e"),addy("239067"),addy("2390ad")},                //registeel
                 {addy("239725"),addy("23972e"),addy("239774")},                //rayquaza
                 {addy("23b032"),addy("23b040"),addy("23b095")},                //kyogre
                 {addy("23b103"),addy("23b111"),addy("23b166")},                //groudon       15
                 {addy("242ba7")},                                              //latios ***
                 {addy("242bba"),addy("161bb8")},                               //latias
                 {addy("242d1b"),addy("242d29")},                               //sudowoodo
                 {addy("267e0d"),addy("267e47"),addy("267e9c"),addy("267ea7")}, //mew
                 {addy("267fe7"),addy("267ff7"),addy("268041"),addy("26804c")}, //deoxys        20
                 {addy("26919f"),addy("2691ce"),addy("26921d"),addy("269228")}, //ho-oh
                 {addy("2692e7"),addy("2692f2"),addy("26933c"),addy("269347")}, //lugia
                 {addy("270058"),addy("27005b"),addy("2700e7")},                //castform
                 {addy("272384"),addy("27238d")} };                             //kecleon 2
    }

    public String[] getTradePokemonNames(){
        return new String[]{"MOOK","SUBWOOFER","MISSINGNO.","DANGERZONE","ABED","BORIS","SCHIGGY","BOX FILLER",
        "DIGIMON","HM SLAVE","BAD TRADE","POOTIS","STELLAAAA","TORVOLD","CLARK","SHARKFACE","GRUMP","Bad EGG",
        "WENTWORTH","POOKYLIPS"};
    }

    public int[] getPokemonListGame(){
        int[] out = new int[386];
        for (int i=0;i<251;i++){
            out[i] = i+1;
        }
        for (int i=252;i<=386;i++){
            out[i-1] = i+25;
        }
        return out;
    }

    public int[] getBattleUseItems(){
        return new int[] {13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,38,44,45,73,74,75,76,77,78,79};
    }

    public int[] getBattleHeldItems(){
        return new int[] {133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,168,169,170,171,172,173,174,179,
            180,181,183,185,186,187,188,191,196,198,200,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,
            219,220,221,222,223,224,225};
    }

    public int[] getEvoCount(){
        return new int[] {
        0,
        2,1,0,2,1,0,2,1,0,2,1,0,2,1,0,2,1,0,1,0,1,0,1,0,1,0,1,0,2,1,0,2,1,0,1,0,1,0,1,0,2,1,2,1,0,1,0,1,0,1,0,1,0,1,0,1,
        0,1,0,2,1,0,2,1,0,2,1,0,2,1,0,1,0,2,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,2,1,0,1,1,0,1,0,1,0,1,0,1,0,0,0,0,1,0,1,0,
        1,0,0,2,1,1,0,1,0,0,1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1,0,1,0,0,0,0,0,0,2,1,0,0,0, // Gen 1
        2,1,0,2,1,0,2,1,0,1,0,1,0,1,0,1,0,0,1,0,2,2,2,1,0,1,0,2,1,0,0,1,0,0,0,2,1,0,0,1,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,1,0,0,0,0,0,0,1,0,1,0,1,0,0,1,0,0,0,0,1,0,0,1,0,0,0,0,1,0,1,1,1,0,0,0,0,0,2,1,0,0,0,0, //Gen 2
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, //25 empty slots
        2,1,0,2,1,0,2,1,0,1,0,1,0,2,1,0,1,0,2,1,0,2,1,0,1,0,1,0,2,1,0,1,0,1,0,2,1,0,1,0,0,2,1,0,1,0,2,0,1,0,0,0,2,1,0,1,
        0,1,0,0,0,0,0,0,1,0,1,0,1,0,1,0,0,1,0,0,2,1,0,1,0,1,0,0,0,0,0,1,0,1,0,1,0,1,0,1,0,1,0,0,0,1,0,1,0,0,0,0,1,1,0,2,
        1,0,1,0,0,0,0,2,1,0,2,1,0,0,0,0,0,0,0,0,0,0,0 //Gen 3
        };
    }

    public int[] getFindableItems(){
        return new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,
        36,37,38,44,45,48,49,50,51,63,64,65,66,67,68,69,70,71,73,74,75,76,77,78,79,80,81,83,84,85,86,93,94,95,96,97,98,103,
        104,106,107,108,109,110,111,168,169,170,171,172,173,174,175,179,180,181,182,183,184,185,186,187,188,189,190,191,192,
        193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,//220,221,  --incense
        222,223,224,225,254,255,256,257,258,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,
        309,310,311,312,313,314,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,
        338};
    }

    public int[] getUsableItems(){
        return new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,
        30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,63,64,65,66,67,68,69,70,71,73,
        74,75,76,77,78,79,80,81,83,84,85,86,93,94,95,96,97,98,103,104,106,107,108,109,110,111,133,134,135,
        136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,
        160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,179,180,181,182,183,184,185,186,
        187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,
        211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,254,255,256,257,258,289,290,291,292,
        293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,313,314,315,316,
        317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338};
    }

    public int[] getSleepOnlyAttacks(){
        return new int[] {138,171}; //Dream Eater, Nightmare, 173,214 - Snore, Sleep Talk
    }

    public int[] getSleepInducingAttacks(){
        return new int[] {47,79,95,142,147,281,290,320};
    }

    public int[] getBabies(){
        return new int[]{171,172,173,235,237,238,239,324,334}; //BABIES.
    }

    public ArrayList<Integer> getArrayListInt(int[] source){
        ArrayList<Integer> out = new ArrayList<Integer>();
        for (int item : source){
            out.add(item);
        }
        return out;
    }

    public ArrayList<String> getArrayListString(String[] source){
        ArrayList<String> out = new ArrayList<String>();
        for (String item : source){
            out.add(item);
        }
        return out;
    }

    public int[] getAttackList(){ //missing: 15,19,57,70,127,148,249,291 (HMs), 165 (struggle), 254,255,256 (stockpile, swallow, spit up)
        return new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,16,17,18,20,21,22,23,24,25,26,27,28,29,30,31,32,33,
        34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,58,59,60,61,62,63,64,65,66,67,
        68,69,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,
        102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,
        128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,149,150,151,152,153,154,
        155,156,157,158,159,160,161,162,163,164,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,
        182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,
        208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,
        234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,250,251,252,253,257,258,259,260,
        261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,
        287,288,289,290,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,
        313,314,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,
        339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354};
    }

    public int[] getAttackListMostlyFull(){ //missing: 165 (struggle), 254,255,256 (stockpile, swallow, spit up)
        return new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,
        34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,
        68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,
        102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,
        128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,
        155,156,157,158,159,160,161,162,163,164,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,
        182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,
        208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,
        234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,257,258,259,260,
        261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,
        287,288,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,
        313,314,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,
        339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354};
    }

    public int[] getAttackListDamaging(){
        return new int[] {1,2,3,4,5,6,7,8,9,10,11,13,16,17,20,21,22,23,24,25,26,27,29,30,31,33,34,35,36,37,38,
        40,41,42,44,49,51,52,53,55,56,58,59,60,61,62,63,64,65,66,67,69,71,72,75,76,80,82,83,84,85,87,88,89,91,
        93,94,98,99,101,118,121,122,123,124,125,126,128,129,130,131,132,136,140,141,143,145,146,149,152,154,155,
        157,158,161,163,167,168,172,175,177,179,181,183,185,188,189,190,192,196,198,200,202,205,209,210,211,216,
        217,218,221,222,223,224,225,228,229,231,232,233,237,238,239,242,245,246,247,250,251,253,257,263,264,265,
        276,279,280,282,284,290,292,295,296,299,301,302,304,305,306,307,308,309,310,311,314,315,317,318,323,324,
        325,326,327,328,330,331,332,333,337,338,340,341,342,343,344,345,348,350,351,352,354};
    }

    public int[] getLegendaryArrayGame(){//game order (Chimecho last)
        return new int[] {144,145,146,150,151,243,244,245,249,250,251,401,402,403,404,405,406,407,408,409,410};
    }

    public int[] getLegendaryArrayDex(){//dex order
        return new int[] {144,145,146,150,151,243,244,245,249,250,251,377,378,379,380,381,382,383,384,385,386};
    }

    public int[][] getBaseColors(){
        return new int[][] {        {255,157,231},{203,149,253},{197,154,139},
                                    {164,106,68},{201,101,111},{235,166,90},
                                    {119,183,183},{113,165,251},{142,89,64},
                                    {159,240,79},{149,173,63},{234,234,8},
                                    {236,8,236},{36,255,36},{151,135,184},
                                    {146,182,133},{108,120,140},{146,129,103},
                                    {216,172,124},{168,162,119},{211,163,90},
                                    {74,74,74},{9,9,187},{146,12,12},
                                    {192,192,192},{250,213,43},{226,82,33},
                                    {130,252,230},{196,208,210},{122,186,250},
                                    {64,69,252},{10,170,170},{97,209,165},
                                    {253,130,47},{238,11,11},{228,135,31},
                                    {9,183,9},{78,145,49},{214,193,50},
                                    {197,77,242},{206,96,159},{130,48,184},
                                    {135,41,250},{66,68,138},{77,95,157},
                                    {245,237,89},{32,245,245},{250,24,24},
                                    {216,61,65},{140,53,53},{53,64,189}};
    }

    public int[] getHoennPokemonGame(){//These are in order of ROM, not the dex
        return new int[] {
            277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,297,298,299,300,304,305,309,310,
            392,393,394,311,312,306,307,364,365,366,63,64,65,301,302,303,370,371,372,335,336,118,119,129,130,350,183,184,74,
            75,76,320,315,316,41,42,169,72,73,322,355,382,383,384,66,67,68,356,357,337,338,353,354,81,82,100,101,386,387,43,
            44,45,182,84,85,363,367,368,330,331,313,314,339,340,218,219,321,88,89,109,110,351,352,27,28,308,227,332,333,334,
            344,345,358,359,380,379,348,349,323,324,326,327,318,319,388,389,390,391,174,39,40,328,329,385,120,121,317,377,
            378,361,362,369,411,376,37,38,172,25,26,54,55,360,202,177,178,203,231,232,127,214,111,112,346,347,341,342,343,
            373,374,375,381,222,170,171,325,116,117,230,395,396,397,398,399,400,401,402,403,407,408,404,405,406,409,410
        };
    }

    public int[] getBasicPokemonGame(){//These are in order of ROM, not the dex
        return new int[] {
        1,4,7,10,13,16,19,21,23,25,27,29,32,35,37,39,41,43,46,48,50,52,54,56,58,60,63,
        66,69,72,74,77,79,81,83,84,86,88,90,92,95,96,98,100,102,104,106,107,108,109,111,113,114,115,116,118,120,122,123,124,
        125,126,127,128,129,131,132,133,137,138,140,142,143,144,145,146,147,150,151,
        152,155,158,161,163,165,167,170,172,173,174,175,177,179,183,185,187,190,191,193,194,198,200,201,202,203,204,206,207,
        209,211,213,214,215,216,218,220,222,223,225,226,227,228,231,234,235,236,237,238,239,240,241,243,244,245,246,249,250,
        251,
        277,280,283,286,288,290,295,298,301,304,306,308,309,311,313,315,317,318,320,321,322,323,325,326,328,330,332,335,337,
        339,341,344,346,348,349,350,351,353,354,355,356,358,360,361,363,364,367,369,370,373,376,377,379,380,381,382,385,386,
        387,388,390,392,395,398,401,402,403,404,405,406,407,408,409,410,411};
    }

    public int[] getBasicPokemonDex(){//These are in order of pokedex, not ROM
        return new int[] {
        1,4,7,10,13,16,19,21,23,25,27,29,32,35,37,39,41,43,46,48,50,52,54,56,58,60,63,66,69,72,74,77,79,81,83,84,86,88,90,92,
        95,96,98,100,102,104,106,107,108,109,111,113,114,115,116,118,120,122,123,124,125,126,127,128,129,131,132,133,137,138,
        140,142,143,144,145,146,147,150,151,
        152,155,158,161,163,165,167,170,172,173,174,175,177,179,183,185,187,190,191,193,194,198,200,201,202,203,204,206,207,
        209,211,213,214,215,216,218,220,222,223,225,226,227,228,231,234,235,236,237,238,239,240,241,243,244,245,246,249,250,
        251,
        252,255,258,261,263,265,270,273,276,278,280,283,285,287,290,293,296,298,299,300,302,303,304,307,309,311,312,313,314,
        315,316,318,320,322,324,325,327,328,331,333,335,336,337,338,339,341,343,345,347,349,351,352,353,355,357,358,359,360,
        361,363,366,369,370,371,374,377,378,379,380,381,382,383,384,385,386};
    }

    public int[][] getFamilyTreeLegacy(){
        return new int[][] {{2,3},{1,3},{1,2},{5,6},{4,6},{4,5},{8,9},{7,9},{7,8},{11,12},{10,12},{10,11},
        {14,15},{13,15},{13,14},{17,18},{16,18},{16,17},{20},{19},{22},{21},{24},{23},{172,26},{172,25},{28},{27},
        {30,31},{29,31},{29,30},{33,34},{32,34},{32,33},{173,36},{173,35},{38},{37},{174,40},{174,39},{42,169},{41,169},
        {44,45,182},{43,45,182},{43,44},{47},{46},{49},{48},{51},{50},{53},{52},{55},{54},{57},{56},{59},{58},{61,62,186},
        {60,62,186},{60,61},{64,65},{63,65},{63,64},{67,68},{66,68},{66,67},{70,71},{69,71},{69,70},{73},{72},{75,76},
        {74,76},{74,75},{78},{77},{80,199},{79},{82},{81},{},{85},{84},{87},{86},{89},{88},{91},{90},{93,94},{92,94},
        {92,93},{208},{97},{96},{99},{98},{101},{100},{103},{102},{105},{104},{236},{236},{},{110},{109},{112},{111},
        {242},{},{},{117,230},{116,230},{119},{118},{121},{120},{},{212},{238},{239},{240},{},{},{130},{129},{},{},
        {134,135,136,196,197},{133},{133},{133},{233},{139},{138},{141},{140},{},{},{},{},{},{148,149},{147,149},
        {147,148},{},{},{153,154},{152,154},{152,153},{156,157},{155,157},{155,156},{159,160},{158,160},{158,159},
        {162},{161},{164},{163},{166},{165},{168},{167},{41,42},{171},{170},{25,26},{35,36},{39,40},{176},{175},{178},
        {177},{180,181},{179,181},{179,180},{43,44},{350,184},{350,183},{},{60,61},{188,189},{187,189},{187,188},{},{192},
        {191},{},{195},{194},{133},{133},{},{79},{},{},{360},{},{205},{204},{},{},{95},{210},{209},{},{123},{},{},{},
        {217},{216},{219},{218},{221},{220},{},{224},{223},{},{},{},{229},{228},{116,117},{232},{231},{137},{},{},
        {106,107,237},{236},{124},{125},{126},{},{113},{},{},{},{247,248},{246,248},{246,247},{},{},{},
        {0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0}, //missingno. +25
        {278,279},{277,279},{277,278},
        {281,282},{280,282},{280,281},{284,285},{283,285},{283,284},{287},{286},{289},{288},{291,292,293,294},{290,292},
        {290,291},{290,294},{290,293},{296,297},{295,297},{295,296},{299,300},{298,300},{298,299},{302,303},{301},{301},
        {305},{304},{307},{306},{},{310},{309},{312},{311},{314},{313},{316},{315},{},{319},{318},{},{},{},{324},{323},
        {},{327},{326},{329},{328},{331},{330},{333,334},{332,334},{332,333},{336},{335},{338},{337},{340},{339},{342,343},
        {341,343},{341,342},{345},{344},{347},{346},{},{},{183,184},{352},{351},{},{},{},{357},{356},{359},{358},{202},
        {362},{361},{},{365,366},{364,366},{364,365},{368},{367},{},{371,372},{370,372},{370,371},{374,375},{373},{373},{},
        {378},{377},{},{},{},{383,384},{382,384},{382,383},{},{},{},{389},{388},{391},{390},{393,394},{392,394},{392,393},
        {396,397},{395,397},{395,396},{399,400},{398,400},{398,399},{},{},{},{},{},{},{},{},{},{},{}
        }; //in order of hex
    }

    public int[][] getFamilyTree(){
        return new int[][] {{1,2,3},{1,2,3},{1,2,3},{4,5,6},{4,5,6},{4,5,6},{7,8,9},{7,8,9},{7,8,9},{10,11,12},{10,11,12},{10,11,12},
        {13,14,15},{13,14,15},{13,14,15},{16,17,18},{16,17,18},{16,17,18},{19,20},{19,20},{21,22},{21,22},{23,24},{23,24},{172,25,26},
        {172,25,26},{27,28},{27,28},{29,30,31},{29,30,31},{29,30,31},{32,33,34},{32,33,34},{32,33,34},{173,35,36},{173,35,36},{37,38},
        {37,38},{174,39,40},{174,39,40},{41,42,169},{41,42,169},{43,44,45,182},{43,44,45,182},{43,44,45},{46,47},{46,47},{48,49},
        {48,49},{50,51},{50,51},{52,53},{52,53},{54,55},{54,55},{56,57},{56,57},{58,59},{58,59},{60,61,62,186},
        {60,61,62,186},{60,61,62},{63,64,65},{63,64,65},{63,64,65},{66,67,68},{66,67,68},{66,67,68},{69,70,71},{69,70,71},{69,70,71},
        {72,73},{72,73},{74,75,76},{74,75,76},{74,75,76},{77,78},{77,78},{79,80,199},{79,80},{81,82},{81,82},{83},{84,85},{84,85},
        {86,87},{86,87},{88,89},{88,89},{90,91},{90,91},{92,93,94},{92,93,94},{92,93,94},{95,208},{96,97},{96,97},{98,99},{98,99},
        {100,101},{100,101},{102,103},{102,103},{104,105},{104,105},{236,106},{236,107},{108},{109,110},{109,110},{111,112},{111,112},
        {113,242},{114},{115},{116,117,230},{116,117,230},{118,119},{118,119},{120,121},{120,121},{122},{123,212},{238,124},{239,125},
        {240,126},{127},{128},{129,130},{129,130},{131},{132},{133,134,135,136,196,197},{133,134},{133,135},{133,136},{137,233},
        {138,139},{138,139},{140,141},{140,141},{142},{143},{144},{145},{146},{147,148,149},{147,148,149},{147,148,149},{150},{151},

        {152,153,154},{152,153,154},{152,153,154},{155,156,157},{155,156,157},{155,156,157},{158,159,160},{158,159,160},{158,159,160},
        {161,162},{161,162},{163,164},{163,164},{165,166},{166,165},{167,168},{167,168},{41,42,169},{170,171},{170,171},{172,25,26},
        {173,35,36},{174,39,40},{175,176},{175,176},{177,178},{177,178},{179,180,181},{179,180,181},{179,180,181},{43,44,182},
        {350,183,184},{350,183,184},{185},{60,61,186},{187,188,189},{187,188,189},{187,188,189},{190},{191,192},{191,192},{193},
        {194,195},{194,195},{133,196},{133,197},{198},{79,199},{200},{201},{360,202},{203},{204,205},{204,205},{206},{207},{95,208},
        {209,210},{209,210},{211},{123,212},{213},{214},{215},{216,217},{216,217},{218,219},{218,219},{220,221},{220,221},{222},
        {223,224},{223,224},{225},{226},{227},{228,229},{228,229},{116,117,230},{231,232},{231,232},{137,233},{234},{235},
        {236,106,107,237},{236,237},{124,238},{125,239},{126,240},{241},{113,242},{243},{244},{245},{246,247,248},{246,247,248},
        {246,247,248},{249},{250},{251},

        {0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0},{0}, //missingno. +25

        {277,278,279},{277,278,279},{277,278,279},{280,281,282},{280,281,282},{280,281,282},{283,284,285},{283,284,285},{283,284,285},
        {286,287},{286,287},{288,289},{288,289},{290,291,292,293,294},{290,291,292},{290,291,292},{290,293,294},{290,293,294},
        {295,296,297},{295,296,297},{295,296,297},{298,299,300},{298,299,300},{298,299,300},{301,302,303},{301,302},{301,303},
        {304,305},{304,305},{306,307},{306,307},{308},{309,310},{309,310},{311,312},{311,312},{313,314},{313,314},{315,316},{315,316},
        {317},{318,319},{318,319},{320},{321},{322},{323,324},{323,324},{325},{326,327},{326,327},{328,329},{328,329},{330,331},
        {330,331},{332,333,334},{332,333,334},{332,333,334},{335,336},{335,336},{337,338},{337,338},{339,340},{339,340},{341,342,343},
        {341,342,343},{341,342,343},{344,345},{344,345},{346,347},{346,347},{348},{349},{350,183,184},{351,352},{351,352},{353},{354},
        {355},{356,357},{356,357},{358,359},{358,359},{360,202},{361,362},{361,362},{363},{364,365,366},{364,365,366},{364,365,366},
        {367,368},{367,368},{369},{370,371,372},{370,371,372},{370,371,372},{373,374,375},{373,374},{373,375},{376},{377,378},
        {377,378},{379},{380},{381},{382,383,384},{382,383,384},{382,383,384},{385},{386},{387},{388,389},{388,389},{390,391},{390,391},
        {392,393,394},{392,393,394},{392,393,394},{395,396,397},{395,396,397},{395,396,397},{398,399,400},{398,399,400},{398,399,400},
        {401},{402},{403},{404},{405},{406},{407},{408},{409},{410},{411}
        }; //in order of hex
    }

    public int[] getPkmnDexToGameTranscription(){
        return new int[]{
            1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
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
    }

    public int[] getPkmnGameToDexTranscription(){
        return new int[]{
            1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
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
    }

    public String[] getPokemonMenuList(){
        return new String[] {"000 - Random","001 - Bulbasaur","002 - Ivysaur","003 - Venusaur","004 - Charmander","005 - Charmeleon",
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
        "379 - Registeel","380 - Latias","381 - Latios","382 - Kyogre","383 - Groudon","384 - Rayquaza","385 - Jirachi","386 - Deoxys"};
    }

    public String[] getPokemonListGameOrder(){
        return new String[]{"BULBASAUR","IVYSAUR","VENUSAUR","CHARMANDER","CHARMELEON","CHARIZARD","SQUIRTLE","WARTORTLE",
        "BLASTOISE","CATERPIE","METAPOD","BUTTERFREE","WEEDLE","KAKUNA","BEEDRILL","PIDGEY","PIDGEOTTO","PIDGEOT","RATTATA",
        "RATICATE","SPEAROW","FEAROW","EKANS","ARBOK","PIKACHU","RAICHU","SANDSHREW","SANDSLASH","NIDORAN♀","NIDORINA",
        "NIDOQUEEN","NIDORAN♂","NIDORINO","NIDOKING","CLEFAIRY","CLEFABLE","VULPIX","NINETALES","JIGGLYPUFF","WIGGLYTUFF",
        "ZUBAT","GOLBAT","ODDISH","GLOOM","VILEPLUME","PARAS","PARASECT","VENONAT","VENOMOTH","DIGLETT","DUGTRIO","MEOWTH",
        "PERSIAN","PSYDUCK","GOLDUCK","MANKEY","PRIMEAPE","GROWLITHE","ARCANINE","POLIWAG","POLIWHIRL","POLIWRATH","ABRA",
        "KADABRA","ALAKAZAM","MACHOP","MACHOKE","MACHAMP","BELLSPROUT","WEEPINBELL","VICTREEBEL","TENTACOOL","TENTACRUEL",
        "GEODUDE","GRAVELER","GOLEM","PONYTA","RAPIDASH","SLOWPOKE","SLOWBRO","MAGNEMITE","MAGNETON","FARFETCH'D","DODUO",
        "DODRIO","SEEL","DEWGONG","GRIMER","MUK","SHELLDER","CLOYSTER","GASTLY","HAUNTER","GENGAR","ONIX","DROWZEE","HYPNO",
        "KRABBY","KINGLER","VOLTORB","ELECTRODE","EXEGGCUTE","EXEGGUTOR","CUBONE","MAROWAK","HITMONLEE","HITMONCHAN",
        "LICKITUNG","KOFFING","WEEZING","RHYHORN","RHYDON","CHANSEY","TANGELA","KANGASKHAN","HORSEA","SEADRA","GOLDEEN",
        "SEAKING","STARYU","STARMIE","MR.MIME","SCYTHER","JYNX","ELECTABUZZ","MAGMAR","PINSIR","TAUROS","MAGIKARP",
        "GYARADOS","LAPRAS","DITTO","EEVEE","VAPOREON","JOLTEON","FLAREON","PORYGON","OMANYTE","OMASTAR","KABUTO",
        "KABUTOPS","AERODACTYL","SNORLAX","ARTICUNO","ZAPDOS","MOLTRES","DRATINI","DRAGONAIR","DRAGONITE","MEWTWO","MEW",
        "CHIKORITA","BAYLEEF","MEGANIUM","CYNDAQUIL","QUILAVA","TYPHLOSION","TOTODILE","CROCONAW","FERALIGATR","SENTRET",
        "FURRET","HOOTHOOT","NOCTOWL","LEDYBA","LEDIAN","SPINARAK","ARIADOS","CROBAT","CHINCHOU","LANTURN","PICHU","CLEFFA",
        "IGGLYBUFF","TOGEPI","TOGETIC","NATU","XATU","MAREEP","FLAAFFY","AMPHAROS","BELLOSSOM","MARILL","AZUMARILL",
        "SUDOWOODO","POLITOED","HOPPIP","SKIPLOOM","JUMPLUFF","AIPOM","SUNKERN","SUNFLORA","YANMA","WOOPER","QUAGSIRE",
        "ESPEON","UMBREON","MURKROW","SLOWKING","MISDREAVUS","UNOWN","WOBBUFFET","GIRAFARIG","PINECO","FORRETRESS",
        "DUNSPARCE","GLIGAR","STEELIX","SNUBBULL","GRANBULL","QWILFISH","SCIZOR","SHUCKLE","HERACROSS","SNEASEL",
        "TEDDIURSA","URSARING","SLUGMA","MAGCARGO","SWINUB","PILOSWINE","CORSOLA","REMORAID","OCTILLERY","DELIBIRD",
        "MANTINE","SKARMORY","HOUNDOUR","HOUNDOOM","KINGDRA","PHANPY","DONPHAN","PORYGON2","STANTLER","SMEARGLE","TYROGUE",
        "HITMONTOP","SMOOCHUM","ELEKID","MAGBY","MILTANK","BLISSEY","RAIKOU","ENTEI","SUICUNE","LARVITAR","PUPITAR",
        "TYRANITAR","LUGIA","HO-OH","CELEBI","???","???","???","???","???","???","???","???","???","???","???","???",
        "???","???","???","???","???","???","???","???","???","???","???","???","???",
        "TREECKO","GROVYLE","SCEPTILE","TORCHIC","COMBUSKEN","BLAZIKEN","MUDKIP",
        "MARSHTOMP","SWAMPERT","POOCHYENA","MIGHTYENA","ZIGZAGOON","LINOONE","WURMPLE","SILCOON","BEAUTIFLY","CASCOON",
        "DUSTOX","LOTAD","LOMBRE","LUDICOLO","SEEDOT","NUZLEAF","SHIFTRY","NINCADA","NINJASK","SHEDINJA","TAILLOW",
        "SWELLOW","SHROOMISH","BRELOOM","SPINDA","WINGULL","PELIPPER","SURSKIT","MASQUERAIN","WAILMER","WAILORD","SKITTY",
        "DELCATTY","KECLEON","BALTOY","CLAYDOL","NOSEPASS","TORKOAL","SABLEYE","BARBOACH","WHISCASH","LUVDISC","CORPHISH",
        "CRAWDAUNT","FEEBAS","MILOTIC","CARVANHA","SHARPEDO","TRAPINCH","VIBRAVA","FLYGON","MAKUHITA","HARIYAMA",
        "ELECTRIKE","MANECTRIC","NUMEL","CAMERUPT","SPHEAL","SEALEO","WALREIN","CACNEA","CACTURNE","SNORUNT","GLALIE",
        "LUNATONE","SOLROCK","AZURILL","SPOINK","GRUMPIG","PLUSLE","MINUN","MAWILE","MEDITITE","MEDICHAM","SWABLU",
        "ALTARIA","WYNAUT","DUSKULL","DUSCLOPS","ROSELIA","SLAKOTH","VIGOROTH","SLAKING","GULPIN","SWALOT","TROPIUS",
        "WHISMUR","LOUDRED","EXPLOUD","CLAMPERL","HUNTAIL","GOREBYSS","ABSOL","SHUPPET","BANETTE","SEVIPER","ZANGOOSE",
        "RELICANTH","ARON","LAIRON","AGGRON","CASTFORM","VOLBEAT","ILLUMISE","LILEEP","CRADILY","ANORITH","ARMALDO",
        "RALTS","KIRLIA","GARDEVOIR","BAGON","SHELGON","SALAMENCE","BELDUM","METANG","METAGROSS","REGIROCK","REGICE",
        "REGISTEEL","KYOGRE","GROUDON","RAYQUAZA","LATIAS","LATIOS","JIRACHI","DEOXYS","CHIMECHO"};
    }

    public String[] getItemList(){
        return new String[]{"---","Master Ball","Ultra Ball","Great Ball","Poké Ball","Safari Ball","Net Ball",
        "Dive Ball","Nest Ball","Repeat Ball","Timer Ball","Luxury Ball","Premier Ball","Potion","Antidote","Burn Heal",
        "Ice Heal","Awakening","Parlyz Heal","Full Restore","Max Potion","Hyper Potion","Super Potion","Full Heal","Revive",
        "Max Revive","Fresh Water","Soda Pop","Lemonade","Moomoo Milk","Energypowder","Energy Root","Heal Powder",
        "Revival Herb","Ether","Max Ether","Elixir","Max Elixir","Lava Cookie","Blue Flute","Yellow Flute","Red Flute",
        "Black Flute","White Flute","Berry Juice","Sacred Ash","Shoal Salt","Shoal Shell","Red Shard","Blue Shard",
        "Yellow Shard","Green Shard","????????","????????","????????","????????","????????","????????","????????","????????",
        "????????","????????","????????","HP Up","Protein","Iron","Carbos","Calcium","Rare Candy","PP Up","Zinc","PP Max",
        "????????","Guard Spec.","Dire Hit","X Attack","X Defend","X Speed","X Accuracy","X Special","Poké Doll",
        "Fluffy Tail","????????","Super Repel","Max Repel","Escape Rope","Repel","????????","????????","????????","????????",
        "????????","????????","Sun Stone","Moon Stone","Fire Stone","ThunderStone","Water Stone","Leaf Stone","????????",
        "????????","????????","????????","TinyMushroom","Big Mushroom","????????","Pearl","Big Pearl","Stardust",
        "Star Piece","Nugget","Heart Scale","????????","????????","????????","????????","????????","????????","????????",
        "????????","????????","Orange Mail","Harbor Mail","Glitter Mail","Mech Mail","Wood Mail","Wave Mail","Bead Mail",
        "Shadow Mail","Tropic Mail","Dream Mail","Fab Mail","Retro Mail","Cheri Berry","Chesto Berry","Pecha Berry",
        "Rawst Berry","Aspear Berry","Leppa Berry","Oran Berry","Persim Berry","Lum Berry","Sitrus Berry","Figy Berry",
        "Wiki Berry","Mago Berry","Aguav Berry","Iapapa Berry","Razz Berry","Bluk Berry","Nanab Berry","Wepear Berry",
        "Pinap Berry","Pomeg Berry","Kelpsy Berry","Qualot Berry","Hondew Berry","Grepa Berry","Tamato Berry","Cornn Berry",
        "Magost Berry","Rabuta Berry","Nomel Berry","Spelon Berry","Pamtre Berry","Watmel Berry","Durin Berry","Belue Berry",
        "Liechi Berry","Ganlon Berry","Salac Berry","Petaya Berry","Apicot Berry","Lansat Berry","Starf Berry",
        "Enigma Berry","????????","????????","????????","BrightPowder","White Herb","Macho Brace","Exp. Share","Quick Claw",
        "Soothe Bell","Mental Herb","Choice Band","King's Rock","Silverpowder","Amulet Coin","Cleanse Tag","Soul Dew",
        "DeepSeaTooth","DeepSeaScale","Smoke Ball","Everstone","Focus Band","Lucky Egg","Scope Lens","Metal Coat",
        "Leftovers","Dragon Scale","Light Ball","Soft Sand","Hard Stone","Miracle Seed","BlackGlasses","Black Belt","Magnet",
        "Mystic Water","Sharp Beak","Poison Barb","NeverMeltIce","Spell Tag","TwistedSpoon","Charcoal","Dragon Fang",
        "Silk Scarf","Up-grade","Shell Bell","Sea Incense","Lax Incense","Lucky Punch","Metal Powder","Thick Club","Stick",
        "????????","????????","????????","????????","????????","????????","????????","????????","????????","????????",
        "????????","????????","????????","????????","????????","????????","????????","????????","????????","????????",
        "????????","????????","????????","????????","????????","????????","????????","????????","Red Scarf","Blue Scarf",
        "Pink Scarf","Green Scarf","Yellow Scarf","Mach Bike","Coin Case","Itemfinder","Old Rod","Good Rod","Super Rod",
        "S.S. Ticket","Contest Pass","????????","Wailmer Pail","Devon Goods","Soot Sack","Basement Key","Acro Bike","Case",
        "Letter","Eon Ticket","Red Orb","Blue Orb","Scanner","Go-Goggles","Meteorite","Rm. 1 Key","Rm. 2 Key","Rm. 4 Key",
        "Rm. 6 Key","Storage Key","Root Fossil","Claw Fossil","Devon Scope","TM01","TM02","TM03","TM04","TM05","TM06",
        "TM07","TM08","TM09","TM10","TM11","TM12","TM13","TM14","TM15","TM16","TM17","TM18","TM19","TM20","TM21","TM22",
        "TM23","TM24","TM25","TM26","TM27","TM28","TM29","TM30","TM31","TM32","TM33","TM34","TM35","TM36","TM37","TM38",
        "TM39","TM40","TM41","TM42","TM43","TM44","TM45","TM46","TM47","TM48","TM49","TM50","HM01","HM02","HM03","HM04",
        "HM05","HM06","HM07","HM08"};
    }

    public String[] getTypeList(){
        return new String[]{"NRM","FTG","FLY","PSN","GRD","RCK","BUG","GHO","STL",
                            "???","FIR","WAT","GRS","ELE","PSY","ICE","DRG","DRK"};
    }

    public String[] getAbilityList(){
        return new String[]{"---","Stench","Drizzle","Speed Boost","Battle Armor",
            "Sturdy","Damp","Limber","Sand Veil","Static","Volt Absorb","Water Absorb","Oblivious","Cloud Nine",
            "Compoundeyes","Insomnia","Color Change","Immunity","Flash Fire","Shield Dust","Own Tempo","Suction Cups",
            "Intimidate","Shadow Tag","Rough Skin","Wonder Guard","Levitate","Effect Spore","Synchronize","Clear Body",
            "Natural Cure","Lightningrod","Serene Grace","Swift Swim","Chlorophyll","Illuminate","Trace","Huge Power",
            "Poison Point","Inner Focus","Magma Armor","Water Veil","Magnet Pull","Soundproof","Rain Dish",
            "Sand Stream","Pressure","Thick Fat","Early Bird","Flame Body","Run Away","Keen Eye","Hyper Cutter",
            "Pickup","Truant","Hustle","Cute Charm","Plus","Minus","Forecast","Sticky Hold","Shed Skin","Guts",
            "Marvel Scale","Liquid Ooze","Overgrow","Blaze","Torrent","Swarm","Rock Head","Drought","Arena Trap",
            "Vital Spirit","White Smoke","Pure Power","Shell Armor","Cacophony","Air Lock"};
    }

    public String[] getAttackListText(){
        return new String[]{"(none)","POUND","KARATE CHOP","DOUBLESLAP","COMET PUNCH","MEGA PUNCH","PAY DAY","FIRE PUNCH",
        "ICE PUNCH","THUNDERPUNCH","SCRATCH","VICEGRIP","GUILLOTINE","RAZOR WIND","SWORDS DANCE","CUT","GUST","WING ATTACK",
        "WHIRLWIND","FLY","BIND","SLAM","VINE WHIP","STOMP","DOUBLE KICK","MEGA KICK","JUMP KICK","ROLLING KICK","SAND-ATTACK",
        "HEADBUTT","HORN ATTACK","FURY ATTACK","HORN DRILL","TACKLE","BODY SLAM","WRAP","TAKE DOWN","THRASH","DOUBLE-EDGE",
        "TAIL WHIP","POISON STING","TWINEEDLE","PIN MISSILE","LEER","BITE","GROWL","ROAR","SING","SUPERSONIC","SONICBOOM",
        "DISABLE","ACID","EMBER","FLAMETHROWER","MIST","WATER GUN","HYDRO PUMP","SURF","ICE BEAM","BLIZZARD","PSYBEAM",
        "BUBBLEBEAM","AURORA BEAM","HYPER BEAM","PECK","DRILL PECK","SUBMISSION","LOW KICK","COUNTER","SEISMIC TOSS","STRENGTH",
        "ABSORB","MEGA DRAIN","LEECH SEED","GROWTH","RAZOR LEAF","SOLARBEAM","POISONPOWDER","STUN SPORE","SLEEP POWDER",
        "PETAL DANCE","STRING SHOT","DRAGON RAGE","FIRE SPIN","THUNDERSHOCK","THUNDERBOLT","THUNDER WAVE","THUNDER","ROCK THROW",
        "EARTHQUAKE","FISSURE","DIG","TOXIC","CONFUSION","PSYCHIC","HYPNOSIS","MEDITATE","AGILITY","QUICK ATTACK","RAGE",
        "TELEPORT","NIGHT SHADE","MIMIC","SCREECH","DOUBLE TEAM","RECOVER","HARDEN","MINIMIZE","SMOKESCREEN","CONFUSE RAY",
        "WITHDRAW","DEFENSE CURL","BARRIER","LIGHT SCREEN","HAZE","REFLECT","FOCUS ENERGY","BIDE","METRONOME","MIRROR MOVE",
        "SELFDESTRUCT","EGG BOMB","LICK","SMOG","SLUDGE","BONE CLUB","FIRE BLAST","WATERFALL","CLAMP","SWIFT","SKULL BASH",
        "SPIKE CANNON","CONSTRICT","AMNESIA","KINESIS","SOFTBOILED","HI JUMP KICK","GLARE","DREAM EATER","POISON GAS","BARRAGE",
        "LEECH LIFE","LOVELY KISS","SKY ATTACK","TRANSFORM","BUBBLE","DIZZY PUNCH","SPORE","FLASH","PSYWAVE","SPLASH","ACID ARMOR",
        "CRABHAMMER","EXPLOSION","FURY SWIPES","BONEMERANG","REST","ROCK SLIDE","HYPER FANG","SHARPEN","CONVERSION","TRI ATTACK",
        "SUPER FANG","SLASH","SUBSTITUTE","STRUGGLE","SKETCH","TRIPLE KICK","THIEF","SPIDER WEB","MIND READER","NIGHTMARE",
        "FLAME WHEEL","SNORE","CURSE","FLAIL","CONVERSION 2","AEROBLAST","COTTON SPORE","REVERSAL","SPITE","POWDER SNOW","PROTECT",
        "MACH PUNCH","SCARY FACE","FAINT ATTACK","SWEET KISS","BELLY DRUM","SLUDGE BOMB","MUD-SLAP","OCTAZOOKA","SPIKES",
        "ZAP CANNON","FORESIGHT","DESTINY BOND","PERISH SONG","ICY WIND","DETECT","BONE RUSH","LOCK-ON","OUTRAGE","SANDSTORM",
        "GIGA DRAIN","ENDURE","CHARM","ROLLOUT","FALSE SWIPE","SWAGGER","MILK DRINK","SPARK","FURY CUTTER","STEEL WING","MEAN LOOK",
        "ATTRACT","SLEEP TALK","HEAL BELL","RETURN","PRESENT","FRUSTRATION","SAFEGUARD","PAIN SPLIT","SACRED FIRE","MAGNITUDE",
        "DYNAMICPUNCH","MEGAHORN","DRAGONBREATH","BATON PASS","ENCORE","PURSUIT","RAPID SPIN","SWEET SCENT","IRON TAIL","METAL CLAW",
        "VITAL THROW","MORNING SUN","SYNTHESIS","MOONLIGHT","HIDDEN POWER","CROSS CHOP","TWISTER","RAIN DANCE","SUNNY DAY","CRUNCH",
        "MIRROR COAT","PSYCH UP","EXTREMESPEED","ANCIENTPOWER","SHADOW BALL","FUTURE SIGHT","ROCK SMASH","WHIRLPOOL","BEAT UP",
        "FAKE OUT","UPROAR","STOCKPILE","SPIT UP","SWALLOW","HEAT WAVE","HAIL","TORMENT","FLATTER","WILL-O-WISP","MEMENTO","FACADE",
        "FOCUS PUNCH","SMELLINGSALT","FOLLOW ME","NATURE POWER","CHARGE","TAUNT","HELPING HAND","TRICK","ROLE PLAY","WISH","ASSIST",
        "INGRAIN","SUPERPOWER","MAGIC COAT","RECYCLE","REVENGE","BRICK BREAK","YAWN","KNOCK OFF","ENDEAVOR","ERUPTION","SKILL SWAP",
        "IMPRISON","REFRESH","GRUDGE","SNATCH","SECRET POWER","DIVE","ARM THRUST","CAMOUFLAGE","TAIL GLOW","LUSTER PURGE","MIST BALL",
        "FEATHERDANCE","TEETER DANCE","BLAZE KICK","MUD SPORT","ICE BALL","NEEDLE ARM","SLACK OFF","HYPER VOICE","POISON FANG",
        "CRUSH CLAW","BLAST BURN","HYDRO CANNON","METEOR MASH","ASTONISH","WEATHER BALL","AROMATHERAPY","FAKE TEARS","AIR CUTTER",
        "OVERHEAT","ODOR SLEUTH","ROCK TOMB","SILVER WIND","METAL SOUND","GRASSWHISTLE","TICKLE","COSMIC POWER","WATER SPOUT",
        "SIGNAL BEAM","SHADOW PUNCH","EXTRASENSORY","SKY UPPERCUT","SAND TOMB","SHEER COLD","MUDDY WATER","BULLET SEED","AERIAL ACE",
        "ICICLE SPEAR","IRON DEFENSE","BLOCK","HOWL","DRAGON CLAW","FRENZY PLANT","BULK UP","BOUNCE","MUD SHOT","POISON TAIL","COVET",
        "VOLT TACKLE","MAGICAL LEAF","WATER SPORT","CALM MIND","LEAF BLADE","DRAGON DANCE","ROCK BLAST","SHOCK WAVE","WATER PULSE",
        "DOOM DESIRE","PSYCHO BOOST"};
    }

    public Character[] getSymbolList(){
        return new Character[]{' ','#', ':', '?', '!', '.', '`', '\'','-', '$', '@', ',', '♀', '♂','<', '>','(',')','/','~','^','*'};
    }

    public Integer[] getSymbolTranscriptionList(){
        return new Integer[]{0, 254, 240, 172, 171, 173, 179, 180, 174, 251, 27,  184, 182, 181, 83,  84, 92, 93,186,176,250,255};
    }
     
    private int addy(String inhex){ //119044
        String hex;
        if (inhex.length()%2 == 1){
            hex = '0' + inhex;
        } else{
            hex = inhex;
        }
        int result = 0;
        int mult = 1;
        for (int i=hex.length();i>0;i-=2,mult *= 256){
            result += mult * hexToInt(hex.substring(i-2,i));
        }
        return result;
    }

    private int hexToInt(String str){
        return ((int)((byte)Integer.valueOf(str, 16).intValue()) & 0xFF);
    }
}
